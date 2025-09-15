import os
#sudo apt install python3-pypdf2
import PyPDF2 
import sys
import re

def extract_text_from_pdfs(folder_path, output_file):
    """
    Lee todos los archivos PDF en una carpeta y sus subcarpetas 
    y extrae el texto a un archivo de texto plano.
    """
    pdf_files_to_process = []
    
    # Paso 1: Recolectar todas las rutas de archivos PDF
    for root, dirs, files in os.walk(folder_path):
        for file_name in files:
            if file_name.endswith('.pdf'):
                pdf_path = os.path.join(root, file_name)
                pdf_files_to_process.append(pdf_path)

    # Paso 2: Ordenar la lista de archivos por nombre (alfabéticamente)
    pdf_files_to_process.sort()

    print(pdf_files_to_process)

    # Paso 3: Procesar los archivos en el orden ya establecido
    with open(output_file, 'w', encoding='utf-8') as out_file:
        for pdf_path in pdf_files_to_process:
            print(f"Extrayendo texto de: {pdf_path}")
            try:
                with open(pdf_path, 'rb') as pdf_file:
                    pdf_reader = PyPDF2.PdfReader(pdf_file)
                    num = 0
                    max_pages = len(pdf_reader.pages)
                    
                    for page in pdf_reader.pages:
                        text = page.extract_text()
                        if text:
                            # Eliminar los numeros de pagina del estilo "X / Y"
                            text = re.sub(r'\d+\s*/\s*\d+', '', text).strip()
                            if text:
                                out_file.write(text)
                                out_file.write('\r\n\r\n')
                        
                        num += 1
                        print(f"Página {num} / {max_pages}")
                    print(f"Texto extraído de: {pdf_path}")
            except Exception as e:
                print(f"Error al procesar {pdf_path}: {e}")

if __name__ == "__main__":
    if len(sys.argv) < 3:
        print("Uso: python extract_pdfs.py <carpeta_a_escanear> <archivo_de_salida>")
        sys.exit(1)

    folder_to_scan = sys.argv[1]
    output_text_file = sys.argv[2]

    if not os.path.isdir(folder_to_scan):
        print(f"Error: La carpeta '{folder_to_scan}' no existe.")
        sys.exit(1)

    extract_text_from_pdfs(folder_to_scan, output_text_file)
