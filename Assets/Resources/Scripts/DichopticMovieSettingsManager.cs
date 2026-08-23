using System;
using System.Xml.Serialization;

public class DichopticMovieSettingsManager
{
    public const int DefaultRenderTextureWidth = 1920;
    public const int DefaultRenderTextureHeight = 1080;
    public const float DefaultBlobOpacity = 1f;
    public const float DefaultLuminance = 1f;

    [Serializable]
    public struct DichopticMovieSettingsStruct
    {
        public float EyeBiasValue;
        public float BlobScaleValue;
        public float BlobGreyColorValue;
        public float BlobOpacityValue;
        [XmlIgnore]
        public bool BlobOpacityValueSpecified;
        public float LuminanceValue;
        [XmlIgnore]
        public bool LuminanceValueSpecified;
        public float BlobTimerValue;
        public int RenderTextureWidth;
        public int RenderTextureHeight;
        public bool ShowFrameRate;
    }

    private const string SETTINGS_FILENAME = "DichopticMovieSettings.cfg";

    private DichopticMovieSettingsStruct _dichopticMovieSettings;

    public DichopticMovieSettingsManager(float eyeBiasValue, float blobScaleValue, float blobGreyColorValue,
        float blobTimerValue, int renderTextureWidth = DefaultRenderTextureWidth,
        int renderTextureHeight = DefaultRenderTextureHeight, bool showFrameRate = false,
        float blobOpacityValue = DefaultBlobOpacity, float luminanceValue = DefaultLuminance)
    {
        _dichopticMovieSettings = new DichopticMovieSettingsStruct
        {
            EyeBiasValue = eyeBiasValue,
            BlobScaleValue = blobScaleValue,
            BlobGreyColorValue = blobGreyColorValue,
            BlobOpacityValue = blobOpacityValue,
            BlobOpacityValueSpecified = true,
            LuminanceValue = luminanceValue,
            LuminanceValueSpecified = true,
            BlobTimerValue = blobTimerValue,
            RenderTextureWidth = renderTextureWidth,
            RenderTextureHeight = renderTextureHeight,
            ShowFrameRate = showFrameRate,
        };

        NormalizeRenderTextureResolution();
    }

    public void TryRestore()
    {
        if (!RestoreSettings())
        {
            StoreSettings();
        }
    }

    public void StoreSettings()
    {
        SettingsHandler.StoreSettings(SETTINGS_FILENAME, _dichopticMovieSettings);
    }

    public bool RestoreSettings()
    {
        DichopticMovieSettingsStruct tempStruct = new();
        if (!SettingsHandler.RestoreSettings(SETTINGS_FILENAME, ref tempStruct))
        {
            return false;
        }

        _dichopticMovieSettings = tempStruct;
        bool settingsChanged = NormalizeRenderTextureResolution();
        settingsChanged |= NormalizeBlobVisualSettings();
        if (settingsChanged)
        {
            StoreSettings();
        }

        return true;
    }


    public void SetEyeBiasValue(float value)
    {
        _dichopticMovieSettings.EyeBiasValue = value;
        StoreSettings();
    }

    public void SetBlobScaleValue(float value)
    {
        _dichopticMovieSettings.BlobScaleValue = value;
        StoreSettings();
    }

    public void SetBlobGreyColorValue(float value)
    {
        _dichopticMovieSettings.BlobGreyColorValue = value;
        StoreSettings();
    }

    public void SetBlobOpacityValue(float value)
    {
        _dichopticMovieSettings.BlobOpacityValue = value;
        _dichopticMovieSettings.BlobOpacityValueSpecified = true;
        StoreSettings();
    }

    public void SetLuminanceValue(float value)
    {
        _dichopticMovieSettings.LuminanceValue = value;
        _dichopticMovieSettings.LuminanceValueSpecified = true;
        StoreSettings();
    }

    public void SetBlobTimerValue(float value)
    {
        _dichopticMovieSettings.BlobTimerValue = value;
        StoreSettings();
    }

    public void SetRenderTextureResolution(int width, int height)
    {
        _dichopticMovieSettings.RenderTextureWidth = width;
        _dichopticMovieSettings.RenderTextureHeight = height;
        StoreSettings();
    }

    public void SetShowFrameRate(bool value)
    {
        _dichopticMovieSettings.ShowFrameRate = value;
        StoreSettings();
    }

    public float GetEyeBiasValue()
    {
        return _dichopticMovieSettings.EyeBiasValue;
    }

    public float GetBlobScaleValue()
    {
        return _dichopticMovieSettings.BlobScaleValue;
    }

    public float GetBlobGreyColorValue()
    {
        return _dichopticMovieSettings.BlobGreyColorValue;
    }

    public float GetBlobOpacityValue()
    {
        return _dichopticMovieSettings.BlobOpacityValue;
    }

    public float GetLuminanceValue()
    {
        return _dichopticMovieSettings.LuminanceValue;
    }

    public float GetBlobTimerValue()
    {
        return _dichopticMovieSettings.BlobTimerValue;
    }

    public int GetRenderTextureWidth()
    {
        return _dichopticMovieSettings.RenderTextureWidth;
    }

    public int GetRenderTextureHeight()
    {
        return _dichopticMovieSettings.RenderTextureHeight;
    }

    public bool GetShowFrameRate()
    {
        return _dichopticMovieSettings.ShowFrameRate;
    }

    private bool NormalizeRenderTextureResolution()
    {
        if (RenderResolutionPresets.FindIndex(_dichopticMovieSettings.RenderTextureWidth,
                _dichopticMovieSettings.RenderTextureHeight) >= 0)
        {
            return false;
        }

        _dichopticMovieSettings.RenderTextureWidth = DefaultRenderTextureWidth;
        _dichopticMovieSettings.RenderTextureHeight = DefaultRenderTextureHeight;
        return true;
    }

    private bool NormalizeBlobVisualSettings()
    {
        bool settingsChanged = false;
        if (!_dichopticMovieSettings.BlobOpacityValueSpecified)
        {
            _dichopticMovieSettings.BlobOpacityValue = DefaultBlobOpacity;
            _dichopticMovieSettings.BlobOpacityValueSpecified = true;
            settingsChanged = true;
        }

        if (!_dichopticMovieSettings.LuminanceValueSpecified)
        {
            _dichopticMovieSettings.LuminanceValue = DefaultLuminance;
            _dichopticMovieSettings.LuminanceValueSpecified = true;
            settingsChanged = true;
        }

        return settingsChanged;
    }
}
