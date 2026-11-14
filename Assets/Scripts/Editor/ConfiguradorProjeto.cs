using System.IO;
using EscondeEsconde.Interface;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace EscondeEsconde.Ferramentas
{
    /// <summary>
    /// Menu "Esconde-Esconde" na barra da Unity:
    ///  1) Montar cena do jogo   -> cria Assets/Scenes/Jogo.unity pronta para dar Play
    ///  2) Gerar executável      -> cria Build/EscondeEsconde.exe (Windows)
    /// Também abre a cena do jogo automaticamente quando o projeto é aberto.
    /// </summary>
    [InitializeOnLoad]
    public static class ConfiguradorProjeto
    {
        public const string CAMINHO_CENA = "Assets/Scenes/Jogo.unity";
        public const string CAMINHO_EXE = "Build/EscondeEsconde.exe";

        static ConfiguradorProjeto()
        {
            if (Application.isBatchMode || SessionState.GetBool("EscondeEsconde.CenaAberta", false)) return;
            EditorApplication.delayCall += AbrirCenaAoIniciar;
        }

        private static void AbrirCenaAoIniciar()
        {
            SessionState.SetBool("EscondeEsconde.CenaAberta", true);
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!File.Exists(CAMINHO_CENA)) MontarCena();
            Scene atual = SceneManager.GetActiveScene();
            if (string.IsNullOrEmpty(atual.path) && !atual.isDirty) EditorSceneManager.OpenScene(CAMINHO_CENA);
        }

        [MenuItem("Esconde-Esconde/1. Montar cena do jogo", priority = 1)]
        public static void MontarCena()
        {
            var cena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camera.tag = "MainCamera";
            camera.transform.position = new Vector3(0f, 0f, -10f);
            var cam = camera.GetComponent<Camera>();
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.11f, 0.08f, 0.07f);

            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            new GameObject("Jogo", typeof(ControladorJogo));

            Directory.CreateDirectory(Path.GetDirectoryName(CAMINHO_CENA));
            EditorSceneManager.SaveScene(cena, CAMINHO_CENA);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(CAMINHO_CENA, true) };
            ConfigurarPlayer();
            AssetDatabase.SaveAssets();
            Debug.Log("[Esconde-Esconde] Cena montada em " + CAMINHO_CENA + ". Aperte Play!");
        }

        [MenuItem("Esconde-Esconde/2. Gerar executável (Windows)", priority = 2)]
        public static void GerarExecutavel()
        {
            if (!File.Exists(CAMINHO_CENA)) MontarCena();
            ConfigurarPlayer();

            var opcoes = new BuildPlayerOptions
            {
                scenes = new[] { CAMINHO_CENA },
                locationPathName = CAMINHO_EXE,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };
            BuildReport relatorio = BuildPipeline.BuildPlayer(opcoes);
            if (relatorio.summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[Esconde-Esconde] Executável gerado em {Path.GetFullPath(CAMINHO_EXE)} ({relatorio.summary.totalSize / (1024 * 1024)} MB)");
                if (!Application.isBatchMode) EditorUtility.RevealInFinder(CAMINHO_EXE);
            }
            else
            {
                Debug.LogError("[Esconde-Esconde] Falha ao gerar o executável: " + relatorio.summary.result);
                if (Application.isBatchMode) EditorApplication.Exit(1);
            }
        }

        /// <summary>Usado na linha de comando: Unity -batchmode -executeMethod ...MontarEGerar</summary>
        public static void MontarEGerar()
        {
            MontarCena();
            GerarExecutavel();
        }

        private static void ConfigurarPlayer()
        {
            PlayerSettings.productName = "Esconde-Esconde";
            PlayerSettings.companyName = "Equipe POO";
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            PlayerSettings.defaultScreenWidth = 1600;
            PlayerSettings.defaultScreenHeight = 900;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = true;
            PlayerSettings.visibleInBackground = true;

            var icone = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Arte/Gerada/jogador_retrato.png");
            if (icone != null) PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { icone }, IconKind.Any);
        }
    }
}
