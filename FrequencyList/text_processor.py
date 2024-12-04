import sys
import re
from collections import Counter
import spacy

sys.stdout.reconfigure(encoding='utf-8')

nlp = spacy.load("uk_core_news_sm")

def load_stopwords(file_path):
    with open(file_path, "r", encoding="utf-8") as f:
        return set(word.strip() for word in f.readlines())

def process_text(input_file, output_file, stopwords_file):
    stop_words = load_stopwords(stopwords_file)

    with open(input_file, "r", encoding="utf-8") as f:
        text = f.read()

    words = re.findall(r'\b\w+\b', text.lower())
    filtered_words = [word for word in words if word not in stop_words]

    doc = nlp(" ".join(filtered_words))
    lemmatized_words = [token.lemma_ for token in doc if token.lemma_ != "-PRON-" and token.lemma_ not in stop_words]

    word_counts = Counter(lemmatized_words)

    with open(output_file, "w", encoding="utf-8") as f:
        for word, count in word_counts.most_common():
            if word:
                f.write(f"{word}\t{count}\n")

if __name__ == "__main__":
    input_file = sys.argv[1]
    output_file = sys.argv[2]
    stopwords_file = sys.argv[3]
    process_text(input_file, output_file, stopwords_file)
