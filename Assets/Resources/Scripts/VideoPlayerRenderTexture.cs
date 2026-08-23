using UnityEngine;
using UnityEngine.Video;

namespace Unity.VRTemplate
{
    /// <summary>
    /// Create a RenderTexture for rendering video to a target renderer.
    /// </summary>
    [RequireComponent(typeof(VideoPlayer))]
    public class VideoPlayerRenderTexture : MonoBehaviour
    {
        const string k_ShaderName = "Unlit/Texture";

        [SerializeField]
        [Tooltip("The target Renderer which will display the video.")]
        Renderer m_Renderer;

        [SerializeField]
        Transform m_DichopticFilter;

        [SerializeField]
        RectTransform m_PlayerBackground;

        [SerializeField]
        [Tooltip("The width of the RenderTexture which will be created.")]
        int m_RenderTextureWidth = 1920;

        [SerializeField]
        [Tooltip("The height of the RenderTexture which will be created.")]
        int m_RenderTextureHeight = 1080;

        [SerializeField]
        [Tooltip("The bit depth of the depth channel for the RenderTexture which will be created.")]
        int m_RenderTextureDepth;

        VideoPlayer m_VideoPlayer;
        RenderTexture m_RenderTexture;
        Material m_Material;

        void Awake()
        {
            m_VideoPlayer = GetComponent<VideoPlayer>();
            m_Material = new Material(Shader.Find(k_ShaderName));
            m_Renderer.material = m_Material;
            RecreateRenderTexture();
        }

        public void SetResolution(int width, int height)
        {
            if (width <= 0 || height <= 0)
            {
                Debug.LogError($"Invalid video render resolution: {width}x{height}", this);
                return;
            }

            if (m_RenderTextureWidth == width && m_RenderTextureHeight == height)
            {
                return;
            }

            m_RenderTextureWidth = width;
            m_RenderTextureHeight = height;
            RecreateRenderTexture();
        }

        void RecreateRenderTexture()
        {
            float displayWidth = m_Renderer.transform.localScale.y * m_RenderTextureWidth / m_RenderTextureHeight;

            Vector3 scale = m_Renderer.transform.localScale;
            scale.x = displayWidth;
            m_Renderer.transform.localScale = scale;

            scale = m_DichopticFilter.localScale;
            scale.z = displayWidth;
            m_DichopticFilter.localScale = scale;

            RectTransform videoPlayer = (RectTransform)transform;
            videoPlayer.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, displayWidth);
            RectTransform content = (RectTransform)videoPlayer.parent;
            m_PlayerBackground.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                displayWidth - content.sizeDelta.x);

            ReleaseRenderTexture();

            m_RenderTexture = new RenderTexture(m_RenderTextureWidth, m_RenderTextureHeight, m_RenderTextureDepth)
            {
                name = $"Video RenderTexture {m_RenderTextureWidth}x{m_RenderTextureHeight}",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
            };
            m_RenderTexture.Create();
            m_Material.mainTexture = m_RenderTexture;
            m_VideoPlayer.targetTexture = m_RenderTexture;
        }

        void ReleaseRenderTexture()
        {
            if (m_RenderTexture == null)
            {
                return;
            }

            if (m_VideoPlayer != null && m_VideoPlayer.targetTexture == m_RenderTexture)
            {
                m_VideoPlayer.targetTexture = null;
            }

            if (m_Material != null && m_Material.mainTexture == m_RenderTexture)
            {
                m_Material.mainTexture = null;
            }

            m_RenderTexture.Release();
            Destroy(m_RenderTexture);
            m_RenderTexture = null;
        }

        void OnDestroy()
        {
            ReleaseRenderTexture();
            if (m_Material != null)
            {
                Destroy(m_Material);
                m_Material = null;
            }
        }
    }
}
