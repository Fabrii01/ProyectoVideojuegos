using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

/// <summary>
/// Identidad visual del juego: señalización de equipo de rescate. Títulos con letra de
/// plantilla, textos estilo señal vial, franjas de peligro y esquinas cortadas.
/// </summary>
public static partial class MontajeRescate
{
    private const string DIR_UI = DIR + "/Interfaz";
    private const string DIR_FUENTES = "Assets/Externos/Fuentes/";
    private const string CARACTERES =
        " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~" +
        "¡¿áéíóúüñÁÉÍÓÚÜÑ·°";

    internal static TMP_FontAsset FuenteTitulo => Cargar<TMP_FontAsset>(DIR_UI + "/FuenteTitulo.asset");
    internal static TMP_FontAsset FuenteCondensada => Cargar<TMP_FontAsset>(DIR_UI + "/FuenteCondensada.asset");
    internal static TMP_FontAsset FuenteTexto => Cargar<TMP_FontAsset>(DIR_UI + "/FuenteTexto.asset");
    internal static Sprite SpritePanel => Cargar<Sprite>(DIR_UI + "/Panel.png");
    private static Sprite SpriteFranja => Cargar<Sprite>(DIR_UI + "/FranjaPeligro.png");

    private static void CrearEstilo()
    {
        Directory.CreateDirectory(DIR_UI);
        AssetDatabase.Refresh();

        CrearFuente("FuenteTitulo", "SairaStencilOne-Regular.ttf");
        CrearFuente("FuenteCondensada", "BarlowCondensed-Bold.ttf");
        CrearFuente("FuenteTexto", "Barlow-Medium.ttf");

        CrearSpritePanel();
        CrearSpriteFranja();
    }

    private static void CrearFuente(string nombre, string archivo)
    {
        string ruta = DIR_UI + "/" + nombre + ".asset";
        if (File.Exists(ruta)) return;

        Font fuente = Cargar<Font>(DIR_FUENTES + archivo);
        TMP_FontAsset asset = TMP_FontAsset.CreateFontAsset(fuente, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, false);
        asset.name = nombre;
        AssetDatabase.CreateAsset(asset, ruta);

        asset.TryAddCharacters(CARACTERES, out string faltantes);
        if (!string.IsNullOrEmpty(faltantes)) Debug.LogWarning("[Montaje] " + nombre + " sin los caracteres: " + faltantes);

        // El atlas y el material deben guardarse dentro del recurso.
        asset.atlasTextures[0].name = nombre + " Atlas";
        asset.material.name = nombre + " Material";
        AssetDatabase.AddObjectToAsset(asset.atlasTextures[0], asset);
        AssetDatabase.AddObjectToAsset(asset.material, asset);

        asset.atlasPopulationMode = AtlasPopulationMode.Static;
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
    }

    /// <summary>Panel con dos esquinas cortadas en diagonal, como una placa metálica.</summary>
    private static void CrearSpritePanel()
    {
        const int LADO = 64;
        const int CORTE = 16;
        string ruta = DIR_UI + "/Panel.png";
        if (File.Exists(ruta)) return;

        Texture2D textura = new Texture2D(LADO, LADO, TextureFormat.RGBA32, false);
        for (int y = 0; y < LADO; y++)
        {
            for (int x = 0; x < LADO; x++)
            {
                // Se cortan la esquina superior izquierda y la inferior derecha.
                float arriba = (x + (LADO - 1 - y)) - CORTE;
                float abajo = ((LADO - 1 - x) + y) - CORTE;
                float alfa = Mathf.Clamp01(Mathf.Min(arriba, abajo) + 0.5f);
                textura.SetPixel(x, y, new Color(1f, 1f, 1f, alfa));
            }
        }

        File.WriteAllBytes(ruta, textura.EncodeToPNG());
        AssetDatabase.ImportAsset(ruta);
        ImportarSprite(ruta, new Vector4(20f, 20f, 20f, 20f), false);
    }

    private static void CrearSpriteFranja()
    {
        const int LADO = 32;
        string ruta = DIR_UI + "/FranjaPeligro.png";
        if (File.Exists(ruta)) return;

        Color amarillo = new Color(1f, 0.74f, 0.04f);
        Color negro = new Color(0.07f, 0.07f, 0.08f);

        Texture2D textura = new Texture2D(LADO, LADO, TextureFormat.RGBA32, false);
        for (int y = 0; y < LADO; y++)
        {
            for (int x = 0; x < LADO; x++)
            {
                textura.SetPixel(x, y, (x + y) % LADO < LADO / 2 ? amarillo : negro);
            }
        }

        File.WriteAllBytes(ruta, textura.EncodeToPNG());
        AssetDatabase.ImportAsset(ruta);
        ImportarSprite(ruta, Vector4.zero, true);
    }

    private static void ImportarSprite(string ruta, Vector4 borde, bool repetir)
    {
        TextureImporter importador = (TextureImporter)AssetImporter.GetAtPath(ruta);
        importador.textureType = TextureImporterType.Sprite;
        importador.spriteImportMode = SpriteImportMode.Single;
        importador.spriteBorder = borde;
        importador.alphaIsTransparency = true;
        importador.mipmapEnabled = false;
        importador.wrapMode = repetir ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;

        TextureImporterSettings ajustes = new TextureImporterSettings();
        importador.ReadTextureSettings(ajustes);
        ajustes.spriteMeshType = SpriteMeshType.FullRect;
        importador.SetTextureSettings(ajustes);
        importador.SaveAndReimport();
    }

    /// <summary>Franja amarilla y negra repetida.</summary>
    private static GameObject FranjaPeligro(Transform padre, Vector2 ancla, Vector2 pos, Vector2 tam)
    {
        RectTransform rt = NuevoRect(padre, "FranjaPeligro", ancla, pos, tam);
        Image imagen = rt.gameObject.AddComponent<Image>();
        imagen.sprite = SpriteFranja;
        imagen.type = Image.Type.Tiled;
        imagen.raycastTarget = false;
        return rt.gameObject;
    }

    /// <summary>Remate de franja de peligro en el borde superior de un panel.</summary>
    internal static void Cinta(Transform panel, float ancho)
    {
        FranjaPeligro(panel, new Vector2(0.5f, 1f), new Vector2(8f, 0f), new Vector2(ancho - 40f, 14f));
    }

    private static float Luminancia(Color color)
    {
        return color.r * 0.3f + color.g * 0.59f + color.b * 0.11f;
    }
}
