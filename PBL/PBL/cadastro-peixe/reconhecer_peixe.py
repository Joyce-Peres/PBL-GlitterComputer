import json
import os
import sys
import google.generativeai as genai
from google.generativeai import types

# Configura a API com a chave via variável de ambiente.
# Ex.: setx GEMINI_API_KEY "..."
API_KEY = os.getenv("GEMINI_API_KEY")
if not API_KEY:
    raise RuntimeError("GEMINI_API_KEY não definido. Configure a variável de ambiente para usar a IA.")
genai.configure(api_key=API_KEY)

def analisar_aquario(caminho_foto):
    try:
        with open(caminho_foto, "rb") as arquivo:
            dados_imagem = {
                'mime_type': 'image/jpeg',
                'data': arquivo.read()
            }
    except FileNotFoundError:
        raise Exception(f"Arquivo '{caminho_foto}' não foi encontrado.")

    # 2. Prompt estruturado para engenharia de contexto
    prompt_comando = (
        "Analise a imagem deste aquário doméstico. Identifique a espécie principal de peixe. "
        "Com base na literatura científica de aquarismo para essa espécie, defina os parâmetros ideais "
        "para o sensor DHT22 (temperatura alvo e faixa min/max), para o sensor LDR (luminosidade alvo e faixa min/max em escala de 0 a 100), "
        "e, quando disponível, o intervalo de pH adequado (min/max). "
        "Retorne estritamente um objeto JSON válido seguindo exatamente o modelo: "
        '{"especie": "Nome", "nome_cientifico": "Nome", "dht22_temp_alvo": 25.0, "dht22_temp_min": 24.0, "dht22_temp_max": 27.0, "ldr_luz_alvo": 40, "ldr_luz_min": 20, "ldr_luz_max": 60, "ph_min": 6.5, "ph_max": 7.5}'
    )

    # 3. Atualizado para o modelo de nova geração suportado pela sua chave
    model = genai.GenerativeModel('gemini-2.5-flash')

    response = model.generate_content(
        contents=[prompt_comando, dados_imagem],
        generation_config=types.GenerationConfig(
            response_mime_type="application/json"
        )
    )

    return response.text


def analisar_especie_texto(especie_texto):
    if not especie_texto or not especie_texto.strip():
        raise Exception("Espécie não informada.")

    prompt_comando = (
        "Você é um assistente especialista em aquarismo. "
        "Com base na literatura científica e boas práticas de aquarismo, "
        "dada a espécie informada, defina parâmetros ideais para um aquário doméstico. "
        "Retorne estritamente um objeto JSON válido seguindo exatamente o modelo: "
        '{"especie": "Nome", "nome_cientifico": "Nome", "dht22_temp_alvo": 25.0, "dht22_temp_min": 24.0, "dht22_temp_max": 27.0, "ldr_luz_alvo": 40, "ldr_luz_min": 20, "ldr_luz_max": 60, "ph_min": 6.5, "ph_max": 7.5}'
        "\n\nEspécie informada: " + especie_texto.strip()
    )

    model = genai.GenerativeModel('gemini-2.5-flash')
    response = model.generate_content(
        contents=[prompt_comando],
        generation_config=types.GenerationConfig(
            response_mime_type="application/json"
        )
    )
    return response.text


# ============================================================
# EXECUÇÃO DO TESTE
# ============================================================
if __name__ == "__main__":
    try:
        if len(sys.argv) < 2:
            print("Uso: python reconhecer_peixe.py <caminho_imagem> | --species <texto>", file=sys.stderr)
            sys.exit(2)

        if sys.argv[1] in ("--species", "--especie"):
            if len(sys.argv) < 3:
                print("Uso: python reconhecer_peixe.py --species <texto>", file=sys.stderr)
                sys.exit(2)
            resultado_raw = analisar_especie_texto(sys.argv[2])
        else:
            resultado_raw = analisar_aquario(sys.argv[1])

        dados = json.loads(resultado_raw.strip())
        # Saída estritamente JSON (integração com o PBL)
        print(json.dumps(dados, ensure_ascii=False))
    except Exception as e:
        print(str(e), file=sys.stderr)
        sys.exit(1)
