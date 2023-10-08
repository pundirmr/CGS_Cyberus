// refer to:
//     https://github.com/keijiro/KinoGlitch.git
//     Assets/Kino/Glitch/DigitalGlitch.cs
//     &
//     https://github.com/mao-test-h/URPGlitch

using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public sealed class AnalogGlitchFeature : ScriptableRendererFeature {
    sealed class CustomRenderPass : ScriptableRenderPass {
        static readonly int MainTexID         = Shader.PropertyToID("_MainTex");
        static readonly int ScanLineJitterID  = Shader.PropertyToID("_ScanLineJitter");
        static readonly int VerticalJumpID    = Shader.PropertyToID("_VerticalJump");
        static readonly int HorizontalShakeID = Shader.PropertyToID("_HorizontalShake");
        static readonly int ColorDriftID      = Shader.PropertyToID("_ColorDrift");

        readonly AnalogGlitchFeature feature;
        private RenderTargetIdentifier mainTexture;
        private float vertJumpTime;
        private string profilerTag;

        private Material material     => feature.MaterialInstance;
        private float ScanLineJitter  => feature.ScanLineJitter;
        private float VerticalJump    => feature.VerticalJump;
        private float HorizontalShake => feature.HorizontalShake;
        private float ColorDrift      => feature.ColorDrift;

        public CustomRenderPass(AnalogGlitchFeature gf, string tag) {
            feature     = gf;
            profilerTag = tag;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cam) {
            cmd.GetTemporaryRT(MainTexID, cam.width, cam.height);
            mainTexture = new RenderTargetIdentifier(MainTexID);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData rdata) {
            if (material == null) return;

            // Copy & Set MainTex.
            var cmd           = CommandBufferPool.Get(profilerTag);
            var camera        = rdata.cameraData.camera;
            var activeTexture = camera.activeTexture;
            cmd.Blit(activeTexture, mainTexture);
            material.SetTexture(MainTexID, activeTexture);

            // Calc Glitch.
            {
                vertJumpTime += Time.deltaTime * VerticalJump * 11.3f;

                var sl_thresh = maths.Clamp01(1.0f - ScanLineJitter * 1.2f);
                var sl_disp = 0.002f + maths.Pow(ScanLineJitter, 3) * 0.05f;
                material.SetVector(ScanLineJitterID, new (sl_disp, sl_thresh));

                var vj = new Vector2(VerticalJump, vertJumpTime);
                material.SetVector(VerticalJumpID, vj);

                material.SetFloat(HorizontalShakeID, HorizontalShake * 0.2f);

                var cd = new Vector2(ColorDrift * 0.04f, Time.time * 606.11f);
                material.SetVector(ColorDriftID, cd);
            }

            cmd.Blit(mainTexture, camera.targetTexture, material);

            // Execute CmdBuff.
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd) {
            base.FrameCleanup(cmd);
            cmd.ReleaseTemporaryRT(MainTexID);
        }

        public override void OnCameraCleanup(CommandBuffer cmd) {
            base.OnCameraCleanup(cmd);
            cmd.ReleaseTemporaryRT(MainTexID);
        }
    }

    [SerializeField] private Material mat;
    private CustomRenderPass pass;
    private CameraType camType = CameraType.Game | CameraType.SceneView;
    private Material MaterialInstance;

    [DisableInInspector] public float ScanLineJitter  = 0f;
    [DisableInInspector] public float VerticalJump    = 0f;
    [DisableInInspector] public float HorizontalShake = 0f;
    [DisableInInspector] public float ColorDrift      = 0f;


    public override void Create() {
        pass = new CustomRenderPass(this, name) {
            renderPassEvent = RenderPassEvent.AfterRendering,
        };

        if (MaterialInstance != null) return;
        MaterialInstance = Instantiate(mat);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData rdata) {
        if ((rdata.cameraData.cameraType & camType) == 0) return;
        renderer.EnqueuePass(pass);
    }
}

