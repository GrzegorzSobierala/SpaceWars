using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Game.Rendering
{
    public class BlurRendererFeature : ScriptableRendererFeature
    {
        class BlurLayerRenderPass : ScriptableRenderPass
        {
            private readonly ShaderTagId _shaderTag = new ShaderTagId("UniversalForward");
            private readonly int _blurLayerMask;
            private string _profilerTag;
            private BlurRendererFeature _feature;
            private RTHandle _rtHandle;

            public BlurLayerRenderPass(string profilerTag, int layerMask, BlurRendererFeature feature)
            {
                this._profilerTag = profilerTag;
                this._blurLayerMask = layerMask;
                this._feature = feature;

                RenderTexture renderTexture = _feature.GetRenderTexture();
                if (renderTexture)
                {
                    this._rtHandle = RTHandles.Alloc(renderTexture);
                }
                else
                {
                    this._rtHandle = RTHandles.Alloc(1920, 1080);
                }
            }

            public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
            {
                if (!ShouldCameraTypeDraw())
                {
                    ResetTarget();
                    return;
                }

                RenderTexture renderTexture = _feature.GetRenderTexture();
                if (!_rtHandle.rt || _rtHandle != renderTexture)
                {
                    if (renderTexture)
                    {
                        _rtHandle = RTHandles.Alloc(renderTexture);
                    }
                    else if (!_rtHandle.rt)
                    {
                        _rtHandle = RTHandles.Alloc(Screen.width, Screen.height);
                        Debug.LogError("BlurRendererFeature is null, blur layer won't work", _feature);
                    }
                    else
                    {
                        Debug.LogError("BlurRendererFeature is null, blur layer won't work", _feature);
                    }
                }

                _rtHandle.rt.Release();
                RenderTextureDescriptor descriptor = cameraTextureDescriptor;
                descriptor.graphicsFormat = _feature._graphicFormat;
                descriptor.depthStencilFormat = _feature._depthStencilFormat;
                _rtHandle.rt.descriptor = descriptor;
                _rtHandle.rt.Create();
                ConfigureTarget(_rtHandle, _rtHandle);
                ConfigureClear(ClearFlag.All, Color.clear);
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                CommandBuffer cmd = CommandBufferPool.Get(_profilerTag);

                if (!ShouldCameraTypeDraw())
                {
                    context.ExecuteCommandBuffer(cmd);
                    CommandBufferPool.Release(cmd);
                    return;
                }

                var drawingSettings = CreateDrawingSettings(_shaderTag, ref renderingData, renderingData.cameraData.defaultOpaqueSortFlags);
                var filteringSettings = new FilteringSettings(RenderQueueRange.opaque, _blurLayerMask);

                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }

            private CameraType GetCameraType()
            {
                return Camera.current == null ? CameraType.Game : Camera.current.cameraType;
            }

            private bool ShouldCameraTypeDraw()
            {
                CameraType cameraType = GetCameraType();
                return cameraType == CameraType.Game || cameraType == CameraType.SceneView;
            }
        }

        [SerializeField] private LayerMask _blurLayer;
        [SerializeField] private RenderPassEvent _renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
        [SerializeField] private RenderTexture _blurRenderTexture;
        [SerializeField] private GraphicsFormat _graphicFormat = GraphicsFormat.R8G8B8A8_UNorm;
        [SerializeField] private GraphicsFormat _depthStencilFormat = GraphicsFormat.D16_UNorm;

        private const string RT_ASSET_PATH = "Assets/GitIgnore/RenderTextures/BlurRenderTexture.renderTexture";
        private const string RT_ASSET_PATH_TO_COPY = "Assets/GitIgnore/RenderTextures/ToCopy/BlurRenderTexture.renderTexture";

        private BlurLayerRenderPass _blurLayerRenderPass;

        public override void Create()
        {
            _blurLayerRenderPass = new BlurLayerRenderPass("Render Blur Layer", _blurLayer.value, this)
            {
                renderPassEvent = this._renderPassEvent,
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(_blurLayerRenderPass);
        }

        private RenderTexture GetRenderTexture()
        {
            if(_blurRenderTexture)
            {
                return _blurRenderTexture;
            }

#if UNITY_EDITOR
            _blurRenderTexture = UnityEditor.AssetDatabase.LoadAssetAtPath<RenderTexture>(RT_ASSET_PATH);
            if (_blurRenderTexture)
            {
                Debug.Log($"{nameof(_blurRenderTexture)} loaded from assets");
                return _blurRenderTexture;
            }
#endif

#if UNITY_EDITOR
            string folderPath = System.IO.Path.GetDirectoryName(RT_ASSET_PATH);

            if(!UnityEditor.AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.Log("Error no folder");
                CreateFoldersRecursively(folderPath);
            }

            CreateNewRenderTextureAsset();
            //Asset is not accesable yet
            RenderTexture newRenderTexture = null;

#else
            RenderTexture newRenderTexture = new RenderTexture(1920, 1080, 24);
            Debug.LogError("GRZECHU ERROR: Created temponary render texture for a blur feature" 
                + RT_ASSET_PATH);
#endif

            return _blurRenderTexture = newRenderTexture;

#if UNITY_EDITOR

            void CreateFoldersRecursively(string folderPath)
            {
                // Base case: if the folder already exists, nothing to do.
                if (UnityEditor.AssetDatabase.IsValidFolder(folderPath))
                    return;

                // Get the parent folder and the folder name.
                string parentFolder = System.IO.Path.GetDirectoryName(folderPath);
                string newFolderName = System.IO.Path.GetFileName(folderPath);

                // Ensure the parent folder exists first.
                if (!UnityEditor.AssetDatabase.IsValidFolder(parentFolder))
                {
                    CreateFoldersRecursively(parentFolder);
                }

                // Create the new folder inside the parent folder.
                UnityEditor.AssetDatabase.CreateFolder(parentFolder, newFolderName);
            }

            /// <summary>
            /// Replaces an existing RenderTexture asset with a new one while preserving its GUID.
            /// </summary>
            void CreateNewRenderTextureAsset()
            {
                string fullPath = System.IO.Path.Combine(Application.dataPath,
                    RT_ASSET_PATH.Substring("Assets/".Length));
                string toCopyFullPath = System.IO.Path.Combine(Application.dataPath,
                    RT_ASSET_PATH_TO_COPY.Substring("Assets/".Length));
                string metaFullFilePath = fullPath + ".meta";

                if (!System.IO.File.Exists(toCopyFullPath))
                {
                    Debug.LogError("Asset to copy not found at: " + fullPath);
                    return;
                }

                System.IO.File.Copy(toCopyFullPath, fullPath, true);

                System.IO.File.WriteAllText(metaFullFilePath,
                    System.IO.File.ReadAllText(toCopyFullPath + ".meta.txt"));

                UnityEditor.AssetDatabase.ImportAsset(RT_ASSET_PATH,
                    UnityEditor.ImportAssetOptions.ForceUpdate);
                UnityEditor.AssetDatabase.Refresh();

                Debug.Log("RenderTexture asset replaced successfully while preserving its meta with GUID.");
            }
#endif
        }
    }
}
