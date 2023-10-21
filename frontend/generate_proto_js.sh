INPUT_DIR="../backend/src/GrpcService/Protos"
OUTPUT_DIR="src/jsproto"

if [ -d INPUT_DIR ]; then
    rm -r $INPUT_DIR
fi

protoc -I=$INPUT_DIR budget_service.proto \
    --plugin=./node_modules/.bin/protoc-gen-ts_proto \
    --ts_proto_out=$OUTPUT_DIR