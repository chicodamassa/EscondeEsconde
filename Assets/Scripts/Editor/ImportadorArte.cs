using UnityEditor;
using UnityEngine;

namespace EscondeEsconde.Ferramentas
{
    /// <summary>
    /// Configura automaticamente toda imagem colocada em Assets/Resources/Arte como Sprite
    /// (sem mipmaps, boa qualidade). Basta trocar o PNG que a Unity reimporta sozinha.
    /// </summary>
    public class ImportadorArte : AssetPostprocessor
    {
        public override uint GetVersion() => 2;

        private void OnPreprocessTexture()
        {
            if (!assetPath.Replace('\\', '/').Contains("/Resources/Arte/")) return;

            var importador = (TextureImporter)assetImporter;
            importador.textureType = TextureImporterType.Sprite;
            importador.spriteImportMode = SpriteImportMode.Single;
            importador.mipmapEnabled = false;
            importador.alphaIsTransparency = true;
            importador.maxTextureSize = 2048;
            importador.textureCompression = TextureImporterCompression.CompressedHQ;
            importador.filterMode = FilterMode.Bilinear;
            importador.wrapMode = TextureWrapMode.Clamp;
        }
    }
}
