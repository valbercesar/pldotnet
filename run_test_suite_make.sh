#!/bin/bash
docker stop $(docker ps -a -q -f name=pldotnet) && \
docker rm $(docker ps -a -q -f name=pldotnet)
docker-compose -f docker-compose-make.yml build && \
docker-compose -f docker-compose-make.yml up