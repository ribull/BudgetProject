INPUT_DIR="../backend/src/GrpcService/Protos"
OUTPUT_DIR="src/jsproto"

if [ -d INPUT_DIR ]; then
    rm -r $INPUT_DIR
fi

protoc -I=$INPUT_DIR budget_service.proto \
    --js_out=import_style=commonjs,binary:$OUTPUT_DIR \
    --grpc-web_out=import_style=typescript,mode=grpcweb:$OUTPUT_DIR