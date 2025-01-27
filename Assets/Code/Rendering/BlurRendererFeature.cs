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

                if (_feature._blurRenderTexture)
                {
                    this._rtHandle = RTHandles.Alloc(feature._blurRenderTexture);
                }
                else
                {
                    this._rtHandle = RTHandles.Alloc(Screen.width, Screen.height);
                }
            }

            public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
            {
                if (!ShouldCameraTypeDraw())
                {
                    ResetTarget();
                    return;
                }

                if (!_rtHandle.rt || _rtHandle != _feature._blurRenderTexture)
                {
                    if (_feature._blurRenderTexture)
                    {
                        _rtHandle = RTHandles.Alloc(_feature._blurRenderTexture);
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
    }
}
