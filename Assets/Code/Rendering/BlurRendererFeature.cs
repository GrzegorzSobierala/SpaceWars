using UnityEngine;
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
            private RenderTargetHandle tempRenderTarget;
            private string profilerTag;

            public BlurLayerRenderPass(string profilerTag, int layerMask)
            {
                this.profilerTag = profilerTag;
                this.blurLayerMask = layerMask;
                tempRenderTarget.Init("_BlurLayerTexture");
            }

            public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
            {
                cmd.GetTemporaryRT(tempRenderTarget.id, cameraTextureDescriptor);
                ConfigureTarget(tempRenderTarget.Identifier());
                ConfigureClear(ClearFlag.All, Color.clear);
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                CommandBuffer cmd = CommandBufferPool.Get(profilerTag);

                var drawingSettings = CreateDrawingSettings(shaderTag, ref renderingData, SortingCriteria.CommonOpaque);
                var filteringSettings = new FilteringSettings(RenderQueueRange.opaque, blurLayerMask);

                cmd.SetGlobalTexture("_CameraDepthTexture", tempRenderTarget.Identifier());

                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);
                context.ExecuteCommandBuffer(cmd);

                CommandBufferPool.Release(cmd);
            }

            public override void FrameCleanup(CommandBuffer cmd)
            {
                if (tempRenderTarget != RenderTargetHandle.CameraTarget)
                {
                    cmd.ReleaseTemporaryRT(tempRenderTarget.id);
                }
            }
        }

        [SerializeField] private LayerMask blurLayer;
        [SerializeField] private RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;

        private BlurLayerRenderPass blurLayerRenderPass;

        public override void Create()
        {
            blurLayerRenderPass = new BlurLayerRenderPass("Render Blur Layer", blurLayer.value)
            {
                renderPassEvent = this.renderPassEvent
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(blurLayerRenderPass);
        }
    }
}
