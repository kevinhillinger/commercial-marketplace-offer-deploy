#! /bin/bash

# Call this while in the ./api directory
cd ./api
out_folder=../pkg/api
autorest README.md --go --go-sdk-folder=$out_folder --tag=preview-2023-03-01

# remove any module file
cd $out_folder
rm go.mod