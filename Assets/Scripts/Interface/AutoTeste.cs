using System;
using System.Collections;
using System.IO;
using EscondeEsconde.Modelo;
using UnityEngine;

namespace EscondeEsconde.Interface
{
    /// <summary>
    /// Ferramenta de teste: só é ativada quando o executável recebe "-autoteste".
    /// Joga uma partida sozinha e salva capturas de tela de cada etapa
    /// (em "-capturas &lt;pasta&gt;" ou na pasta de dados do jogo).
    /// </summary>
    public class AutoTeste : MonoBehaviour
    {
        private ControladorJogo c;
        private string pasta;

        private IEnumerator Start()
        {
            c = GetComponent<ControladorJogo>();
            string[] args = Environment.GetCommandLineArgs();
            int i = Array.IndexOf(args, "-capturas");
            pasta = i >= 0 && i + 1 < args.Length ? args[i + 1] : Path.Combine(Application.persistentDataPath, "capturas");
            Directory.CreateDirectory(pasta);
            Debug.Log("[AutoTeste] salvando capturas em " + pasta);

            int j = Array.IndexOf(args, "-esconderijo");
            string idEsconderijo = j >= 0 && j + 1 < args.Length ? args[j + 1] : "sala_sofa";
            Dificuldade dificuldade = Array.IndexOf(args, "-dificil") >= 0 ? Dificuldade.Dificil : Dificuldade.Normal;

            yield return new WaitForSeconds(1.5f);
            yield return Capturar("01_menu");

            c.IniciarJogo("Tester", dificuldade);
            yield return new WaitForSeconds(2.4f);
            yield return Capturar("02_entrada");

            c.TelaEsconder.DefinirDepuracaoAreas(true);
            yield return Capturar("03_areas_entrada");
            for (int k = 1; k <= 4; k++)
            {
                c.TelaEsconder.Mover(1);
                yield return new WaitForSeconds(0.7f);
                yield return Capturar($"03_areas_{c.Jogador.ComodoAtual.Id}");
            }
            c.TelaEsconder.DefinirDepuracaoAreas(false);

            // anda até o cômodo do esconderijo pedido e se esconde nele
            Esconderijo alvo = null;
            foreach (var comodo in c.Gerenciador.TodosOsComodos)
                if (comodo.ObterEsconderijo(idEsconderijo) != null) alvo = comodo.ObterEsconderijo(idEsconderijo);
            for (int k = 0; k < 5 && c.Jogador.ComodoAtual != alvo.Comodo; k++)
            {
                c.TelaEsconder.Mover(1);
                yield return new WaitForSeconds(0.7f);
            }
            c.TelaEsconder.AbrirDialogo(alvo);
            yield return new WaitForSeconds(0.4f);
            yield return Capturar("04_dialogo");
            c.TelaEsconder.Confirmar();
            yield return new WaitForSeconds(0.5f);
            yield return Capturar("05_escondido");

            c.Gerenciador.PularPreparacao();
            yield return new WaitForSeconds(0.8f);
            yield return Capturar("06_transicao");
            yield return new WaitForSeconds(3.2f);
            yield return Capturar("07_busca_inicio");
            yield return new WaitForSeconds(4f);
            yield return Capturar("08_busca");

            c.Gerenciador.DefinirMultiplicadorVelocidade(4f);
            float limite = Time.time + 90f;
            while (c.Gerenciador.FaseAtual != FaseJogo.Encerrado && Time.time < limite) yield return null;
            yield return new WaitForSeconds(0.4f);
            yield return Capturar("09_fim_busca");
            yield return new WaitForSeconds(5f);
            yield return Capturar("10_resultado");

            Debug.Log($"[AutoTeste] fim: venceu={c.Gerenciador.Venceu} motivo={c.Gerenciador.MotivoFim}");
            Application.Quit();
        }

        private IEnumerator Capturar(string nome)
        {
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(pasta, nome + ".png"));
            yield return null;
            yield return null;
        }
    }
}
