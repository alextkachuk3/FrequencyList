import sys
import re
from collections import Counter
import spacy

sys.stdout.reconfigure(encoding='utf-8')

nlp = spacy.load("uk_core_news_sm")

STOP_WORDS = {
    "я", "ти", "він", "вона", "воно", "ми", "ви", "вони",
    "мене", "тебе", "його", "її", "нас", "вас", "їх",
    "мені", "тобі", "йому", "їй", "нам", "вам", "їм",
    "мною", "тобою", "ним", "нею", "нами", "вами", "ними",
    "себе", "собі", "собою", "хто", "що", "який", "чий",
    "котрий", "скільки", "цей", "такий", "сам", "сама",
    "саме", "весь", "вся", "все", "кожен", "жоден",
    "інший", "будь-який", "ніщо", "ніхто", "усі", "усім",
    
    "в", "у", "на", "до", "з", "із", "під", "над", "про", 
    "за", "перед", "після", "через", "без", "біля", "поблизу",
    "поруч", "навколо", "між", "серед", "задля", "для", 
    "поза", "окрім", "крім", "завдяки", "щодо", "під час",
    
    "і", "й", "та", "але", "або", "чи", "бо", "хоча", 
    "проте", "зате", "що", "щоб", "як", "якби", "немов", 
    "ніби", "тому", "тому що", "оскільки", "поки", "коли", 
    "де", "якщо", "навіть", "аби",
    
    "не", "ні", "хай", "нехай", "от", "таки", "то", "ж", 
    "же", "ось", "аби", "щоб", "чи", "тощо",
    
    "ой", "ех", "ух", "ага", "гей", "ну", "ох", "ах", 
    "ой-ой", "тьфу", "хе", "ха", "гу", "уф", "ой-ля-ля",
    
    "це", "іще", "щось", "десь", "там", "тут", "тому", 
    "сюди", "так", "ні", "дуже", "майже", "зовсім", "потім", 
    "зараз", "тоді", "теж", "також", "лише", "або", "чи",
    "навіть", "ще", "завжди", "ніколи", "хоч", "будь"
}

WORD_REGEX = r"\b\w+(?:['ʼ’]\w+)?\b"

def process_text(input_file, output_file):
    with open(input_file, "r", encoding="utf-8") as f:
        text = f.read()

    words = re.findall(WORD_REGEX, text.lower())
    filtered_words = [word for word in words if word not in STOP_WORDS]

    doc = nlp(" ".join(filtered_words))
    lemmatized_words = [token.lemma_ for token in doc if token.lemma_ not in STOP_WORDS]

    word_counts = Counter(lemmatized_words)

    with open(output_file, "w", encoding="utf-8") as f:
        for word, count in word_counts.most_common():
            if word:
                f.write(f"{word}\t{count}\n")

if __name__ == "__main__":
    input_file = sys.argv[1]
    output_file = sys.argv[2]
    process_text(input_file, output_file)
