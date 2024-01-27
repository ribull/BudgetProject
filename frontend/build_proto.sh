#!/bin/bash
SRC_DIR="$(pwd)/../backend/src/GrpcService/Protos"

protoc \
    --plugin="protoc-gen-ts=$(npm root)/.bin/protoc-gen-ts_proto" \
    --ts_opt=esModuleInterop=true \
    --ts_out="./src/generated" \
    -I=$SRC_DIR \
    $SRC_DIR/*.proto