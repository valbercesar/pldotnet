all: pldotnet-whitepaper-dec22.pdf

# You can manually create the performance graphs in the img/ subdirectory:
#
# To install dependencies:
# pip3 install kaleido numpy pandas plotly
#
# To create graphs:
# for lang in plpython plpgsql pljava plperl pllua pltcl plr; do ./processor.py data.csv plcsharp $lang; done

SMALL_IMGS=img/pldotnet-comparison-pljava.png img/pldotnet-comparison-pllua.png img/pldotnet-comparison-plperl.png img/pldotnet-comparison-plpgsql.png img/pldotnet-comparison-plpython.png img/pldotnet-comparison-plr.png img/pldotnet-comparison-pltcl.png img/pldotnet-comparison-plv8.png 
BIG_IMGS=img/pldotnet-performance-comparison.png img/pldotnet-sequence-diagram.png

img/%.png:
	cd img && basename $@ | xargs make

.venv.created:
	make venvclean # sleazy dep
	mkdir venv
	(cd venv && python3 -m virtualenv .)
	. venv/bin/activate && \
	pip3 install pygments
	touch .venv.created

interior_pdf: .venv.created $(BIG_IMGS)
	. venv/bin/activate && \
	pdflatex -shell-escape pldotnet-whitepaper-dec22.tex # `-shell-escape` is for minted

pldotnet-whitepaper-dec22.pdf: pldotnet-whitepaper-dec22.tex
	(cd img && make)
	make interior_pdf
	bibtex pldotnet-whitepaper-dec22
	rm -f $@
	make interior_pdf

venvclean:
	rm -rf .venv.created venv/

lightclean:
	rm -rf pldotnet-whitepaper-dec22.{aux,out,toc,log,lot,lof} _minted-pldotnet-whitepaper-dec22/

clean: lightclean
	rm -f pldotnet-whitepaper-dec22.pdf
	cd img && make clean

veryclean: venvclean clean
	cd img && make veryclean
