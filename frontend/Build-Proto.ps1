$protoFiles = 'budget_service.proto', 'health_check.proto'

foreach ($protoFile in $protoFiles)
{
    protoc `
    --plugin=protoc-gen-ts_proto=".\\node_modules\\.bin\\protoc-gen-ts_proto.cmd" `
    --ts_proto_opt=esModuleInterop=true `
    --ts_proto_opt=outputServices=grpc-js `
    --ts_proto_out="./src/generated" `
    --proto_path="../backend/src/GrpcService/Protos" `
    $protoFile
}