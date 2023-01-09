#!/usr/bin/env python3

# To install dependencies:
# pip3 install kaleido numpy pandas plotly

# To create graphs:
# for lang in plpython plpgsql pljava plperl pllua pltcl plr plv8; do ./processor.py pldotnet-performance-data.csv plcsharp $lang; done

import os, sys, csv
import plotly.graph_objects as go
import plotly.express as px
import numpy as np
import pandas as pd

########################################
header = "#" * 40

def make_perc(val, inverted=True):
    if inverted:
        val = 1.0 / val
    return "%.2f" % ((val-1.0) * 100.0)

def debug(msg, enabled=True):
    if enabled:
        print("# DEBUG: %s" % msg)

# def ydump(data):
#     print(header)
#     print(yaml.dump(data))
#     print(header)

def average(vals):
    average = (sum(vals) / float(len(vals)))
    debug("Returning average %s" % average, False)
    return average

########################################

def make_av(data):
    sum1 = sum(data[0][1])
    sum2 = sum(data[1][1])
    return sum1/sum2

def make_fig(filename, data):
    av = make_av(data)
    names = [name for (name, datum) in data]
    title = "%s takes %s%% longer than %s" % (names[0], make_perc(av, False), names[1])
    if av < 1:
        title = "%s takes %s%% longer than %s" % (names[1], make_perc(av), names[0])
    # title += "<br>(pl/dotnet is 1.0; less is better)" 
    data2 = dict(data)

    dim = int(os.environ.get("IMGSZ", "512")) # pixel size

    fig = px.line(pd.DataFrame(data2), y=names, log_y=False)

    fig.add_shape(
        type='line',
        x0=0,
        y0=1.0/av,
        x1=len(data[0][1]),
        y1=1.0/av,
        # text=("Average: %s" % (1.0/av)),
        line={ 
              "color": "Green",
              "dash": "dash",
              }
    )

    # a quick hack to make the plpgsql label more visible
    # better would be to also reduce the arrow, but it will do
    yshift=0
    # our performance is now so close that we don't need this!
    # if "plpgsql" in filename:
        # yshift=int(dim * -0.01)

    fig.add_annotation(
                x=(3 * len(data[0][1])/4),
                y=(1.0/av),
                yshift=yshift,
                text=("Average: %.2f%%" %  (100.0/av)),
                # color="Green"
            )

#     fig.data[0].update(
#         annotations=[
#             dict(
#                 x=len(data[0][1])/2,
#                 y=1.0/av,
#                 xref="x",
#                 yref="y",
#                 text=("Average: %.2f" %  (1.0/av)),
#                 showarrow=False,
#                 ax=0,
#                 ay=0
#             )
#         ]
#     )

    fig.update_layout(
        title=title,
        xaxis_title="Test",
        yaxis_title="Relative execution time",
        legend_title="Language",
        title_x=0.5,
        font=dict(
            family="Open Sans",
            size=12,
            color="Black"
        )
    )
    fig.update_xaxes(showticklabels=False)
    fig.update_layout(width=dim, height=dim)

    fig.write_image(filename)

def read_data(filename):
    with open(filename, newline='') as csvfile:
        reader = csv.reader(csvfile, delimiter='\t')
        first = True
        ret = {}
        for row in reader:
            if first:
                header = row
                for pl in row[1:]:
                    ret[pl] = {}
                first = False
            else:
                for (pl, datum) in zip(header[1:], row[1:]):
                    ret[pl][row[0]] = datum
        return ret

def compare(data, lang1, lang2):
    comparison = {}
    for test in sorted(data[lang1].keys()):
        debug("Comparing test %s between languages %s and %s" % \
                (test, lang1, lang2), False)
        if test in data[lang2]:
            if data[lang2][test] not in ('-', ''):
                score1 = float(data[lang1][test])
                score2 = float(data[lang2][test])
                c = score1/score2
                debug("c(%s) = score1(%s)/score2(%s)" %
                      (c, score1, score2), False)
                if c < 10.0 and c > 0.1:
                    comparison[test] = c
    return comparison

def graph_compare(data, l1, l2, inverted=True):
    comparison = compare(data, l1, l2)
    av = average(comparison.values())
    debug("Average 2 is %s" % av, False)
    vals1 = sorted(comparison.values())
    vals2 = [1.0 for x in vals1]
    # This is dumb, but I'm too lazy to fix it properly
    if inverted:
        vals2 = [1.0 / x for x in vals1]
        vals1 = [1.0 for x in vals2]
    data = [
            (l1, vals1),
            (l2, vals2),
            ]
    filename="pldotnet-comparison-%s.png" % l2
    av = make_av(data)
    print("Language %08s takes on average %06s%% as long as %s." % 
          (l2, "%.2f" % (100.0 / av), l1))
    make_fig(filename, data)

def report(comparison):
    for(test, score) in comparison.values():
        pass

########################################
def main(argv):
    filename = argv[1]
    data = read_data(filename)
    # ydump(data)
    comparison = compare(data, argv[2], argv[3])
    # ydump(comparison)
    graph_compare(data, argv[2], argv[3])

main(sys.argv)

