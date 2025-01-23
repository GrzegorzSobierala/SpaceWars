using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Game.Rendering
{
    public class BlurRendererFeature : ScriptableRendererFeature
    {
        class BlurLayerRenderPass : ScriptableRenderPass
        {
            private readonly ShaderTagId shaderTag = new ShaderTagId("UniversalForward");
            private readonly int blurLayerMask;
            private string profilerTag;
            BlurRendererFeature feature;

            int lastCheckFrame = 0;
            int checkFrameCount = 0;

            private int CheckFrameCount()
            {
                if (lastCheckFrame == Time.frameCount)
                {
                    checkFrameCount++;
                }
                else
                {
                    lastCheckFrame = Time.frameCount;
                    checkFrameCount = 1;
                }

                return checkFrameCount;
            }


            public BlurLayerRenderPass(string profilerTag, int layerMask, BlurRendererFeature feature)
            {
                this.profilerTag = profilerTag;
                this.blurLayerMask = layerMask;
                this.feature = feature;
                //RTHandle colorRT = RTHandles.Alloc(feature.blurRenderTexture)
            }

            CameraType GetCameraType()
            {
                return Camera.current == null ? CameraType.Game : Camera.current.cameraType;
            }

            public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
            {
                //GetRenderTextureContent(feature.blurRenderTexture);
                //GetRenderTextureContent(feature.blurRenderTexture);
                //GetRenderTextureContent(feature.debugRenderTexture);

                //Debug.Log($"{GetCameraType()} {colorAttachment}");
                Debug.Log($"1 {GetRenderTextureContent(feature.blurRenderTexture)}");
                if (Camera.current && Camera.current.cameraType != CameraType.SceneView)
                {
                    ResetTarget();
                    Debug.Log($"2 {GetRenderTextureContent(feature.blurRenderTexture)}");
                    return;
                }
                Debug.Log($"2 {GetRenderTextureContent(feature.blurRenderTexture)}");

                if (feature.blurRenderTexture == null)
                {
                    CreateNewDebugRT(cameraTextureDescriptor);
                }
                else
                {
                    feature.blurRenderTexture.Release();
                    feature.blurRenderTexture.descriptor = cameraTextureDescriptor;
                    feature.blurRenderTexture.Create();
                }

                
                ConfigureTarget(feature.blurRenderTexture,feature.blurRenderTexture);
                ConfigureClear(ClearFlag.All, Color.clear);
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {

                if (renderingData.cameraData.cameraType != CameraType.Game && renderingData.cameraData.cameraType != CameraType.SceneView)
                {
                    return;
                }

                CommandBuffer cmd = CommandBufferPool.Get(profilerTag);

                var drawingSettings = CreateDrawingSettings(shaderTag, ref renderingData, SortingCriteria.CommonOpaque);
                var filteringSettings = new FilteringSettings(RenderQueueRange.opaque, blurLayerMask);

                //cmd.SetGlobalTexture("_CameraDepthTexture", feature.blurRenderTexture);

                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);

                //cmd.Blit(feature.blurRenderTexture, feature.debugRenderTexture);

                context.ExecuteCommandBuffer(cmd);

                CommandBufferPool.Release(cmd);
                //GetRenderTextureContent(feature.blurRenderTexture);
            }

            public override void FrameCleanup(CommandBuffer cmd)
            {
                //GetRenderTextureContent(feature.blurRenderTexture);


                if (Camera.current && Camera.current.cameraType != CameraType.SceneView)
                    return;


               // GetRenderTextureContent(feature.blurRenderTexture);
               // GetRenderTextureContent(feature.debugRenderTexture);
                //ResetTarget();
                //Profiler.BeginSample($"{CheckFrameCount()} | Blur FrameCleanup {feature.blurRenderTexture.descriptor.width}");
                //Profiler.EndSample();
            }

            public override void OnFinishCameraStackRendering(CommandBuffer cmd)
            {
                //GetRenderTextureContent(feature.blurRenderTexture);
            }

            public override void OnCameraCleanup(CommandBuffer cmd)
            {
                //GetRenderTextureContent(feature.blurRenderTexture);
            }

            public string GetRenderTextureContent(RenderTexture renderTexture)
            {
                // Ensure the RenderTexture is active
                RenderTexture.active = renderTexture;

                // Create a new Texture2D with the same dimensions as the RenderTexture
                Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);

                // Read the pixels from the RenderTexture into the Texture2D
                texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                texture.Apply();

                // Reset the active RenderTexture
                RenderTexture.active = null;

                //Debug.Log($"{GetCameraType()} {texture.imageContentsHash}");

                return GetCameraType().ToString() + " | " + texture.imageContentsHash.ToString();
            }

            private void CreateNewDebugRT(RenderTextureDescriptor descriptor)
            {
                RenderTexture newDebugRT =
                            new RenderTexture(descriptor);
                newDebugRT.name = "BlurRenderTexture" + feature.name;
                newDebugRT.Create();

                string path = feature.DebugRenderTexturePath + "/" + newDebugRT.name + ".renderTexture";

                // Save the RenderTexture as an asset in the project
                UnityEditor.AssetDatabase.CreateAsset(newDebugRT, path);

                feature.blurRenderTexture = newDebugRT;

                Debug.Log($"RenderTexture created at: {path}");

                UnityEditor.EditorUtility.SetDirty(feature.blurRenderTexture);
                UnityEditor.AssetDatabase.SaveAssetIfDirty(feature.blurRenderTexture);
            }
        }

        [SerializeField] private LayerMask blurLayer;
        [SerializeField] private RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
        [Space]
        public RenderTexture blurRenderTexture;
        public RenderTexture debugRenderTexture;

        public UnityEngine.Object debugRenderTexturePath;
        public string DebugRenderTexturePath
        {
            get
            {
                string result = "Assets";

#if UNITY_EDITOR
                if (debugRenderTexturePath)
                {
                    result = UnityEditor.AssetDatabase.GetAssetPath(debugRenderTexturePath);
                }
#endif

                return result;
            }
        }

        private BlurLayerRenderPass blurLayerRenderPass;

        public override void Create()
        {
            blurLayerRenderPass = new BlurLayerRenderPass("Render Blur Layer", blurLayer.value, this)
            {
                renderPassEvent = this.renderPassEvent,
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(blurLayerRenderPass);
        }
    }
}
