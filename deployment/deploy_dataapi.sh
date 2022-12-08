#!/bin/bash

unzip -q DataAPI.Web.zip
sudo cp /var/www/DataAPI.Web/appsettings.json /var/www/DataAPI.Web/appsettings.json.bak
sudo cp -R publish/* /var/www/DataAPI.Web/
sudo cp /var/www/DataAPI.Web/appsettings.json.bak /var/www/DataAPI.Web/appsettings.json
rm -rf publish
sudo chmod 777 -R /var/www/DataAPI*
sudo systemctl restart dataapi.service

