using System.Collections.Generic;
using UnityEngine;

namespace EscondeEsconde.Interface
{
    /// <summary>
    /// Efeitos sonoros e música gerados por código (síntese simples de ondas),
    /// então o projeto não depende de nenhum arquivo de áudio externo.
    /// </summary>
    public class SomJogo : MonoBehaviour
    {
        public enum Efeito { Clique, Tique, Passo, Porta, Esconder, Coracao, Lupa, Encontrado, Vitoria, Derrota }

        public static SomJogo Instancia { get; private set; }

        private const int TAXA = 44100;
        private AudioSource fonteEfeitos;
        private AudioSource fonteMusica;
        private readonly Dictionary<Efeito, AudioClip> clipes = new Dictionary<Efeito, AudioClip>();
        private AudioClip musica;

        private void Awake()
        {
            Instancia = this;
            fonteEfeitos = gameObject.AddComponent<AudioSource>();
            fonteEfeitos.playOnAwake = false;
            fonteMusica = gameObject.AddComponent<AudioSource>();
            fonteMusica.playOnAwake = false;
            fonteMusica.loop = true;
            fonteMusica.volume = 0.22f;

            clipes[Efeito.Clique] = Gerar("clique", 0.06f, t => Seno(880f, t) * Decaimento(t, 40f) * 0.5f);
            clipes[Efeito.Tique] = Gerar("tique", 0.05f, t => Quadrada(1400f, t) * Decaimento(t, 60f) * 0.25f);
            clipes[Efeito.Passo] = Gerar("passo", 0.07f, t => Ruido() * Decaimento(t, 55f) * 0.18f);
            clipes[Efeito.Porta] = Gerar("porta", 0.18f, t => (Seno(110f, t) * 0.8f + Ruido() * 0.2f) * Decaimento(t, 18f) * 0.6f);
            clipes[Efeito.Esconder] = Gerar("esconder", 0.3f, t => Seno(Mathf.Lerp(700f, 180f, t / 0.3f), t) * Decaimento(t, 9f) * 0.45f);
            clipes[Efeito.Coracao] = Gerar("coracao", 0.45f, t => (Seno(55f, t) * Decaimento(t, 22f) + Seno(55f, Mathf.Max(0f, t - 0.18f)) * Decaimento(Mathf.Max(0f, t - 0.18f), 22f) * (t > 0.18f ? 0.8f : 0f)) * 0.9f);
            clipes[Efeito.Lupa] = Gerar("lupa", 0.25f, t => Seno(520f + 180f * Mathf.Sin(t * 40f), t) * Decaimento(t, 10f) * 0.3f);
            clipes[Efeito.Encontrado] = Melodia("encontrado", new[] { 659f, 523f, 392f, 262f }, 0.16f, true);
            clipes[Efeito.Vitoria] = Melodia("vitoria", new[] { 523f, 659f, 784f, 1047f, 784f, 1047f }, 0.13f, false);
            clipes[Efeito.Derrota] = Melodia("derrota", new[] { 392f, 370f, 349f, 330f }, 0.28f, true);
            musica = GerarMusica();
        }

        public void Tocar(Efeito efeito, float volume = 1f)
        {
            if (clipes.TryGetValue(efeito, out AudioClip clip)) fonteEfeitos.PlayOneShot(clip, volume);
        }

        public void TocarMusica(float tom)
        {
            fonteMusica.pitch = tom;
            if (!fonteMusica.isPlaying)
            {
                fonteMusica.clip = musica;
                fonteMusica.Play();
            }
        }

        public void PararMusica()
        {
            fonteMusica.Stop();
        }

        // ---------------- síntese ----------------
        private static float Seno(float freq, float t) => Mathf.Sin(2f * Mathf.PI * freq * t);
        private static float Quadrada(float freq, float t) => Mathf.Sign(Mathf.Sin(2f * Mathf.PI * freq * t));
        private static float Decaimento(float t, float k) => Mathf.Exp(-k * t);
        private static float Ruido() => Random.value * 2f - 1f;

        private static AudioClip Gerar(string nome, float duracao, System.Func<float, float> onda)
        {
            int n = Mathf.CeilToInt(duracao * TAXA);
            var dados = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / TAXA;
                float fade = Mathf.Clamp01((n - i) / (TAXA * 0.01f)); // evita estalo no final
                dados[i] = onda(t) * fade;
            }
            var clip = AudioClip.Create(nome, n, 1, TAXA, false);
            clip.SetData(dados, 0);
            return clip;
        }

        private static AudioClip Melodia(string nome, float[] notas, float duracaoNota, bool quadrada)
        {
            int porNota = Mathf.CeilToInt(duracaoNota * TAXA);
            var dados = new float[porNota * notas.Length + TAXA / 4];
            for (int k = 0; k < notas.Length; k++)
            {
                float extra = k == notas.Length - 1 ? 2.5f : 1f;
                int len = Mathf.Min(dados.Length - k * porNota, Mathf.CeilToInt(porNota * extra));
                for (int i = 0; i < len; i++)
                {
                    float t = (float)i / TAXA;
                    float onda = quadrada ? Quadrada(notas[k], t) * 0.35f : (Seno(notas[k], t) + 0.3f * Seno(notas[k] * 2f, t)) * 0.6f;
                    float env = Mathf.Clamp01(t * 200f) * Decaimento(t, extra > 1f ? 3f : 7f);
                    dados[k * porNota + i] += onda * env * 0.45f;
                }
            }
            var clip = AudioClip.Create(nome, dados.Length, 1, TAXA, false);
            clip.SetData(dados, 0);
            return clip;
        }

        /// <summary>Música de suspense em loop: baixo "pé ante pé" + notinhas em lá menor (8 compassos).</summary>
        private static AudioClip GerarMusica()
        {
            const float bpm = 104f;
            float colcheia = 60f / bpm / 2f;
            float[] baixo = { 110f, 0f, 130.8f, 0f, 146.8f, 0f, 130.8f, 0f, 110f, 0f, 98f, 0f, 103.8f, 0f, 82.4f, 0f };
            float[] melodia = { 440f, 0f, 0f, 523.3f, 0f, 493.9f, 0f, 0f, 440f, 0f, 392f, 0f, 415.3f, 0f, 0f, 0f,
                                440f, 0f, 0f, 523.3f, 0f, 587.3f, 0f, 523.3f, 493.9f, 0f, 0f, 0f, 415.3f, 0f, 0f, 0f };
            int passos = 64;
            int porPasso = Mathf.RoundToInt(colcheia * TAXA);
            var dados = new float[passos * porPasso];
            for (int p = 0; p < passos; p++)
            {
                float fb = baixo[p % baixo.Length];
                float fm = melodia[p % melodia.Length];
                for (int i = 0; i < porPasso; i++)
                {
                    float t = (float)i / TAXA;
                    float v = 0f;
                    if (fb > 0f) v += Seno(fb, t) * Decaimento(t, 9f) * 0.55f + Seno(fb * 2f, t) * Decaimento(t, 14f) * 0.15f;
                    if (fm > 0f && p >= 16) v += (Seno(fm, t) + 0.25f * Seno(fm * 3f, t)) * Decaimento(t, 12f) * 0.22f;
                    if (p % 4 == 2) v += Ruido() * Decaimento(t, 80f) * 0.08f; // chiadinho
                    dados[p * porPasso + i] = v * 0.6f;
                }
            }
            var clip = AudioClip.Create("musica_suspense", dados.Length, 1, TAXA, false);
            clip.SetData(dados, 0);
            return clip;
        }
    }
}
