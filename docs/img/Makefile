PUML_JAR=/opt/homebrew/Cellar/plantuml/1.2022.14/libexec/plantuml.jar

all: pldotnet-performance-comparison.png pldotnet-sequence-diagram.png heatmap.png

clean:
	rm -rf *png

veryclean: clean venvclean

venvclean:
	rm -rf .venv.created venv/

.venv.created:
	make venvclean
	mkdir venv
	(cd venv && python3 -m virtualenv .)
	. venv/bin/activate && \
	pip3 install kaleido numpy pandas plotly seaborn
	touch .venv.created

pldotnet-performance-comparison.png: .venv.created pldotnet-performance-graph-maker.py pldotnet-performance-data.csv
	. venv/bin/activate && \
	for lang in plfsharp plpython plpgsql pljava plperl pllua pltcl plr plv8; do ./pldotnet-performance-graph-maker.py pldotnet-performance-data.csv plcsharp $$lang; done
	montage pldotnet-comparison-pljava.png pldotnet-comparison-pllua.png pldotnet-comparison-plperl.png pldotnet-comparison-plpgsql.png pldotnet-comparison-plpython.png pldotnet-comparison-plr.png pldotnet-comparison-pltcl.png pldotnet-comparison-plv8.png pldotnet-comparison-plfsharp.png -tile 3x3 -geometry +2+2 pldotnet-performance-comparison.png

heatmap.png: .venv.created pldotnet-performance-heatmap-maker.py pldotnet-performance-data.csv
	. venv/bin/activate && \
	python pldotnet-performance-heatmap-maker.py pldotnet-performance-data.csv $@

pldotnet-sequence-diagram.png: ./pldotnet-sequence-diagram.puml
	plantuml -Tpng ./pldotnet-sequence-diagram.puml

benchmark-tests:
	cd .. && cd .. && make benchmark-tests NRUNS=$(NRUNS) && cd docs/img
