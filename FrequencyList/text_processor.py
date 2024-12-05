import sys
import re
from collections import Counter
import spacy

sys.stdout.reconfigure(encoding='utf-8')

nlp = spacy.load("uk_core_news_sm")

WORD_REGEX = r"\b\w+(?:['ʼ’]\w+)?\b"

def process_text(input_file, output_file):
    with open(input_file, "r", encoding="utf-8") as f:
        text = f.read()

    words = re.findall(WORD_REGEX, text.lower())

    doc = nlp(" ".join(words))
    lemmatized_words = [token.lemma_ for token in doc]

    word_counts = Counter(lemmatized_words)

    with open(output_file, "w", encoding="utf-8") as f:
        for word, count in word_counts.most_common():
            if word:
                f.write(f"{word}\t{count}\n")

if __name__ == "__main__":
    input_file = sys.argv[1]
    output_file = sys.argv[2]
    process_text(input_file, output_file)
