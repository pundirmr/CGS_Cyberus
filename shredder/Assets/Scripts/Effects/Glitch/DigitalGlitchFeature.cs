// refered to:
//     https://github.com/keijiro/KinoGlitch.git
//     Assets/Kino/Glitch/DigitalGlitch.cs
//     &
//     https://github.com/mao-test-h/URPGlitch

using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.TestTools;
using Random = Unity.Mathematics.Random;

public sealed class DigitalGlitchFeature : ScriptableRendererFeature {
    sealed class CustomRenderPass : ScriptableRenderPass {
        static readonly int MainTexID   = Shader.PropertyToID("_MainTex");
        static readonly int NoiseTexID  = Shader.PropertyToID("_NoiseTex");
        static readonly int TrashTexID  = Shader.PropertyToID("_TrashTex");
        static readonly int IntensityID = Shader.PropertyToID("_Intensity");

        readonly DigitalGlitchFeature feature;
        private string profilerTag;
        private Random random;

        private RenderTargetIdentifier mainTexture;
        private Texture2D noiseTexture;
        private RenderTexture trashFrame1;
        private RenderTexture trashFrame2;

        private Color RandomColor {
            get {
                var r = random.NextFloat4();
                return new Color(r.x, r.y, r.z, r.w);
            }
        }

        private Material material => feature.MaterialInstance;
        private float Intensity   => feature.Intensity;

        public CustomRenderPass(DigitalGlitchFeature gf, string tag) {
            feature     = gf;
            random      = new Random((uint) System.DateTime.Now.Ticks);
            profilerTag = tag;

            SetUpResources();
            UpdateNoiseTexture();
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cam) {
            cmd.GetTemporaryRT(MainTexID, cam.width, cam.height);
            mainTexture = new RenderTargetIdentifier(MainTexID);

            if (random.NextFloat() > maths.Lerp(0.9f, 0.5f, Intensity)) {
                UpdateNoiseTexture();
            }
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData rdata) {
            if (material == null) return;

            var cmd           = CommandBufferPool.Get(profilerTag);
            var camera        = rdata.cameraData.camera;
            var activeTexture = camera.activeTexture;
            cmd.Blit(activeTexture, mainTexture);

            // Update trash frames on a constant interval.
            var frameCount = Time.frameCount;
            if (frameCount % 13 == 0) cmd.Blit(activeTexture, trashFrame1);
            if (frameCount % 73 == 0) cmd.Blit(activeTexture, trashFrame2);

            material.SetFloat(IntensityID, Intensity);
            material.SetTexture(NoiseTexID, noiseTexture);
            material.SetTexture(MainTexID, activeTexture);
            material.SetTexture(TrashTexID, random.NextFloat() > 0.5f ? trashFrame1 : trashFrame2);
            cmd.Blit(mainTexture, camera.targetTexture, material);


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

        private void SetUpResources() {
            noiseTexture            = new Texture2D(64, 32, TextureFormat.ARGB32, false);
            noiseTexture.hideFlags  = HideFlags.DontSave;
            noiseTexture.wrapMode   = TextureWrapMode.Clamp;
            noiseTexture.filterMode = FilterMode.Point;

            trashFrame1           = new (Screen.width, Screen.height, 0);
            trashFrame2           = new (Screen.width, Screen.height, 0);
            trashFrame1.hideFlags = HideFlags.DontSave;
            trashFrame2.hideFlags = HideFlags.DontSave;
        }

        private void UpdateNoiseTexture() {
            var color = RandomColor;
            for (int y = 0; y < noiseTexture.height; y++) {
                for (int x = 0; x < noiseTexture.width; x++) {
                    if (random.NextFloat() > 0.89f) {
                        color = RandomColor;
                    }

                    noiseTexture.SetPixel(x, y, color);
                }
            }

            noiseTexture.Apply();
        }
    }

    [SerializeField] private Material mat;
    private CustomRenderPass pass;
    private CameraType camType = CameraType.Game | CameraType.SceneView;
    private Material MaterialInstance;
    
    [DisableInInspector] public float Intensity = 0f;

    public override void Create() {
        pass = new CustomRenderPass(this, name) {
            renderPassEvent = RenderPassEvent.AfterRendering + 1,
        };

        if (MaterialInstance != null) return;
        MaterialInstance = Instantiate(mat);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData rdata) {
        if ((rdata.cameraData.cameraType & camType) == 0) return;
        renderer.EnqueuePass(pass);
    }

}

