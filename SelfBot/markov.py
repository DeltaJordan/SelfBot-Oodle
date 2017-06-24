import markovify
import os

script_dir = os.path.dirname(__file__)

with open(os.path.join(script_dir, "markov.txt"), 'rt', encoding='utf-8') as f:
    text = f.read()

text_model = markovify.NewlineText(text)

with open(os.path.join(script_dir, "output.txt"), "w+", encoding='utf-8') as g:
	g.write(text_model.make_sentence())