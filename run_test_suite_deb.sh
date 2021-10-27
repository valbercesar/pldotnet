#!/bin/bash
docker stop $(docker ps -a -q -f name=pldotnet) && \
docker rm $(docker ps -a -q -f name=pldotnet)
rm -f debian/packages/*.deb
docker-compose -f docker-compose-build.yml build && \
docker-compose -f docker-compose-build.yml up