#!/bin/bash

echo "Generating env files..."

rm -f academy.env staffing.env fe.env

grep '^ACADEMY_' .env | sed 's/^ACADEMY_//' > academy.env
grep '^STAFFING_' .env | sed 's/^STAFFING_//' > staffing.env
grep '^FE_' .env | sed 's/^FE_//' > fe.env

echo "Done"