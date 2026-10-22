using Axis.Infrastructure.Host.Responses;
using NJsonSchema;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace Axis.Infrastructure.Components.Swagger.Internal;

/// <summary>
/// NSwag 操作级处理器，统一把每个 Endpoint 的响应示意为 Axis 框架的 <see cref="ResponseResult"/> 信封。
///
/// <para>做两件事：</para>
/// <list type="number">
/// <item><description><b>包装成功响应</b>：把 2xx 响应的 schema 从 <c>TData</c> 改写为 <c>ResponseResult&lt;TData&gt;</c> 样式（内联展开字段）。</description></item>
/// <item><description><b>注入默认错误响应</b>：为每个 Endpoint 补齐 400 / 401 / 500 的 <see cref="ResponseResult{T}"/> 示例 schema，便于前端在 Knife4UI 里直接看到错误结构。</description></item>
/// </list>
///
/// <para>设计取舍：</para>
/// <list type="bullet">
/// <item><description>不在运行时做 <c>MakeGenericType</c> 去生成真正的 <c>ResponseResult&lt;T&gt;</c> CLR 类型（成本高、触发 AOT 警告），而是以 NSwag 的 <see cref="JsonSchema"/> 内联拼装。</description></item>
/// <item><description>data 字段通过引用 / 复制原 schema 的方式保留类型信息，前端在 Knife4UI 里能下钻到具体 DTO。</description></item>
/// </list>
/// </summary>
public class ResponseEnvelopeOperationProcessor : IOperationProcessor
{
    private const string EnvelopeKeyPrefix = "Axis.ResponseEnvelope.";

    /// <inheritdoc />
    public bool Process(OperationProcessorContext context)
    {
        var operation = context.OperationDescription.Operation;

        WrapSuccessResponses(operation);
        EnsureErrorResponses(operation);

        return true;
    }

    /// <summary>
    /// 将所有 2xx 响应的 schema 替换为 <see cref="ResponseResult{T}"/> 内联信封。
    /// </summary>
    private static void WrapSuccessResponses(OpenApiOperation operation)
    {
        foreach (var (code, response) in operation.Responses.ToList())
        {
            if (!int.TryParse(code, out var status)) continue;
            if (status is < 200 or >= 300) continue;
            if (response.Schema is null) continue;

            // 204 No Content 不包装
            if (status == 204) continue;

            response.Schema = BuildEnvelope(response.Schema, isSuccess: true);
        }
    }

    /// <summary>
    /// 补齐默认错误响应（已有同码时不覆盖，尊重业务显式声明）。
    /// </summary>
    private static void EnsureErrorResponses(OpenApiOperation operation)
    {
        AddIfAbsent(operation, "400", "请求失败 / 业务异常（如 40000 参数错误、40001 校验失败、42200 业务规则拒绝）");
        AddIfAbsent(operation, "401", "未认证（40100）/ Token 过期（40101）/ Token 无效（40102）");
        AddIfAbsent(operation, "403", "无权限（40300）");
        AddIfAbsent(operation, "500", "服务器内部错误（50000）");
    }

    private static void AddIfAbsent(OpenApiOperation operation, string statusCode, string description)
    {
        if (operation.Responses.ContainsKey(statusCode)) return;

        operation.Responses[statusCode] = new OpenApiResponse
        {
            Description = description,
            Schema = BuildEnvelope(dataSchema: null, isSuccess: false)
        };
    }

    /// <summary>
    /// 构造一个 <see cref="ResponseResult{T}"/> 样式的 JSON Schema。
    /// </summary>
    /// <param name="dataSchema">原响应 schema（成功信封的 data 字段）。为 null 时 data 视为 object/null。</param>
    /// <param name="isSuccess">true 渲染为成功示例（code=10000、success=true），false 渲染为错误示例。</param>
    private static JsonSchema BuildEnvelope(JsonSchema dataSchema, bool isSuccess)
    {
        var schema = new JsonSchema
        {
            Type = JsonObjectType.Object,
            Description = isSuccess ? "Axis 统一响应体（成功）" : "Axis 统一响应体（错误）"
        };

        schema.Properties["code"] = new JsonSchemaProperty
        {
            Type = JsonObjectType.Integer,
            Description = "业务码，前 3 位 == HTTP 状态码；成功为 10000",
            Example = isSuccess ? 10000 : 42200
        };
        schema.Properties["success"] = new JsonSchemaProperty
        {
            Type = JsonObjectType.Boolean,
            Description = "是否成功",
            Example = isSuccess
        };
        schema.Properties["message"] = new JsonSchemaProperty
        {
            Type = JsonObjectType.String,
            Description = "人类可读消息；失败时为错误原因",
            Example = isSuccess ? "" : "业务规则拒绝"
        };
        schema.Properties["data"] = BuildDataProperty(dataSchema, isSuccess);
        schema.Properties["traceId"] = new JsonSchemaProperty
        {
            Type = JsonObjectType.String,
            Description = "链路追踪 ID（对应服务端日志 TraceId）"
        };
        schema.Properties["path"] = new JsonSchemaProperty
        {
            Type = JsonObjectType.String,
            Description = "请求路径"
        };
        schema.Properties["timestamp"] = new JsonSchemaProperty
        {
            Type = JsonObjectType.String,
            Format = JsonFormatStrings.DateTime,
            Description = "响应时间，ISO 8601 服务器本地时间"
        };

        schema.RequiredProperties.Add("code");
        schema.RequiredProperties.Add("success");
        schema.RequiredProperties.Add("message");
        schema.RequiredProperties.Add("traceId");
        schema.RequiredProperties.Add("path");
        schema.RequiredProperties.Add("timestamp");

        return schema;
    }

    /// <summary>
    /// 将原响应 schema 包装为 data 字段：
    /// <list type="bullet">
    /// <item><description>原 schema 是 <c>$ref</c> → 保留引用（前端能在 Knife4UI 下钻到具体 DTO）</description></item>
    /// <item><description>原 schema 是内联复合类型 → 通过 <c>allOf</c> 嵌入</description></item>
    /// <item><description>失败响应 → data 为 nullable object</description></item>
    /// </list>
    /// </summary>
    private static JsonSchemaProperty BuildDataProperty(JsonSchema dataSchema, bool isSuccess)
    {
        var prop = new JsonSchemaProperty
        {
            Description = isSuccess ? "业务数据" : "错误附加数据（通常为 null）",
            IsNullableRaw = true
        };

        if (dataSchema is null)
        {
            prop.Type = JsonObjectType.Object | JsonObjectType.Null;
            return prop;
        }

        // 原 schema 通过 $ref 指向 components/schemas/XXX → data 也用 allOf 引用保留类型信息
        if (dataSchema.HasReference || dataSchema.Reference is not null)
        {
            prop.AllOf.Add(dataSchema);
            return prop;
        }

        // 数组类型（列表响应）
        if (dataSchema.Type == JsonObjectType.Array && dataSchema.Item is not null)
        {
            prop.Type = JsonObjectType.Array;
            prop.Item = dataSchema.Item;
            return prop;
        }

        // 内联基础类型（string / int / bool 等）
        if (dataSchema.Type != JsonObjectType.None)
        {
            prop.Type = dataSchema.Type;
            prop.Format = dataSchema.Format;
            return prop;
        }

        // 兜底：用 allOf 合并
        prop.AllOf.Add(dataSchema);
        return prop;
    }
}
