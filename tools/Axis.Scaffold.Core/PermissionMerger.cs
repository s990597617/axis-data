using System.Text;
using System.Text.RegularExpressions;

namespace Axis.Scaffold;

/// <summary>
/// 将模块权限嵌套类 / 前端权限键安全合并进已有码表文件。
/// </summary>
public static class PermissionMerger
{
    public static FileAction MergeBackend(string repoRoot, TokenContext tokens, bool dryRun, bool force)
    {
        var path = Path.Combine(
            repoRoot, "src", "Axis.Domain", tokens.Area, $"{tokens.PermissionCodesType}.cs");
        var relative = Path.GetRelativePath(repoRoot, path);

        if (!File.Exists(path))
        {
            return new FileAction(path, "SKIP", "权限根类不存在（应由 areaPermissionFull 新建）");
        }

        var original = File.ReadAllText(path);
        if (ContainsNestedClass(original, tokens.Module))
        {
            if (!force)
                return new FileAction(path, "SKIP", $"已含 {tokens.Module}（使用 --force 覆盖该嵌套类）");

            original = RemoveNestedClass(original, tokens.Module);
        }

        var nested = BuildBackendNestedClass(tokens);
        var insertAt = FindRootClassCloseBrace(original, tokens.PermissionCodesType);
        var merged = original[..insertAt] + nested + original[insertAt..];

        if (dryRun)
            return new FileAction(path, "MERGE", "dry-run · 后端权限", Content: merged);

        File.WriteAllText(path, merged);
        return new FileAction(path, "MERGE", "后端权限", Content: merged);
    }

    public static FileAction MergeFrontend(string repoRoot, TokenContext tokens, bool dryRun, bool force)
    {
        var path = Path.Combine(
            repoRoot, "frontend", "src", "modules", tokens.AreaLower, "permissions", "codes.ts");

        if (!File.Exists(path))
            return new FileAction(path, "SKIP", "前端 codes.ts 不存在（应由 frontendPermissionFull 新建）");

        var original = File.ReadAllText(path);
        if (ContainsFrontendKey(original, tokens.ModuleCamel))
        {
            if (!force)
                return new FileAction(path, "SKIP", $"已含 {tokens.ModuleCamel}（使用 --force 覆盖该键）");

            original = RemoveFrontendKey(original, tokens.ModuleCamel);
        }

        var entry = $"  {tokens.ModuleCamel}: createCrudPermissions('{tokens.RouteKey}'),";
        var merged = InsertFrontendEntry(original, tokens.PermissionCodesVar, entry);

        if (dryRun)
            return new FileAction(path, "MERGE", "dry-run · 前端权限", Content: merged);

        File.WriteAllText(path, merged);
        return new FileAction(path, "MERGE", "前端权限", Content: merged);
    }

    private static string BuildBackendNestedClass(TokenContext tokens)
    {
        // Order：非 System 从 200 起；System 用 200 避免与现有 10–100 段硬冲突（可日后手调）。
        var order = 200;
        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine($"    /// {tokens.CnName}管理权限。");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine(
            $"    [PermissionModule(\"{tokens.RouteKey}\", \"{tokens.CnName}管理\", Order = {order})]");
        sb.AppendLine($"    public static class {tokens.Module}");
        sb.AppendLine("    {");
        sb.AppendLine("        [PermissionAction(\"查询\", Order = 1)]");
        sb.AppendLine($"        public const string Query = \"{tokens.PermPrefix}:query\";");
        sb.AppendLine();
        sb.AppendLine("        [PermissionAction(\"新增\", Order = 2)]");
        sb.AppendLine($"        public const string Create = \"{tokens.PermPrefix}:create\";");
        sb.AppendLine();
        sb.AppendLine("        [PermissionAction(\"编辑\", Order = 3)]");
        sb.AppendLine($"        public const string Edit = \"{tokens.PermPrefix}:edit\";");
        sb.AppendLine();
        sb.AppendLine("        [PermissionAction(\"删除\", Order = 4)]");
        sb.AppendLine($"        public const string Delete = \"{tokens.PermPrefix}:delete\";");
        sb.AppendLine("    }");
        return sb.ToString();
    }

    private static bool ContainsNestedClass(string content, string module)
        => Regex.IsMatch(content, $@"public\s+static\s+class\s+{Regex.Escape(module)}\b");

    private static bool ContainsFrontendKey(string content, string key)
        => Regex.IsMatch(content, $@"\b{Regex.Escape(key)}\s*:");

    private static int FindRootClassCloseBrace(string content, string className)
    {
        var match = Regex.Match(
            content,
            $@"public\s+static\s+class\s+{Regex.Escape(className)}\s*\{{");
        if (!match.Success)
            throw new InvalidOperationException($"未找到权限根类：{className}");

        var depth = 0;
        for (var i = match.Index + match.Length - 1; i < content.Length; i++)
        {
            var c = content[i];
            if (c == '{') depth++;
            else if (c == '}')
            {
                depth--;
                if (depth == 0) return i;
            }
        }

        throw new InvalidOperationException($"权限根类 {className} 大括号不匹配。");
    }

    private static string RemoveNestedClass(string content, string module)
    {
        // 从紧邻的 summary / 特性 起，到该嵌套类结束。
        var pattern = new Regex(
            $@"(?:\r?\n)?(?:[ \t]*///[^\r\n]*\r?\n)*[ \t]*\[PermissionModule[^\]]*\]\r?\n[ \t]*public\s+static\s+class\s+{Regex.Escape(module)}\s*\{{",
            RegexOptions.Multiline);

        var match = pattern.Match(content);
        if (!match.Success)
        {
            // 降级：仅匹配 class 声明
            pattern = new Regex(
                $@"(?:\r?\n)?[ \t]*public\s+static\s+class\s+{Regex.Escape(module)}\s*\{{",
                RegexOptions.Multiline);
            match = pattern.Match(content);
            if (!match.Success)
                throw new InvalidOperationException($"无法定位待覆盖的嵌套类 {module}。");
        }

        var openBrace = match.Index + match.Length - 1;
        var close = FindMatchingClose(content, openBrace);
        return content[..match.Index] + content[(close + 1)..];
    }

    private static int FindMatchingClose(string content, int openBraceIndex)
    {
        var depth = 0;
        for (var i = openBraceIndex; i < content.Length; i++)
        {
            if (content[i] == '{') depth++;
            else if (content[i] == '}')
            {
                depth--;
                if (depth == 0) return i;
            }
        }

        throw new InvalidOperationException("嵌套类大括号不匹配。");
    }

    private static string RemoveFrontendKey(string content, string key)
    {
        var pattern = new Regex(
            $@"\r?\n?[ \t]*{Regex.Escape(key)}\s*:\s*createCrudPermissions\([^)]*\)\s*,?");
        return pattern.Replace(content, "", 1);
    }

    private static string InsertFrontendEntry(string content, string exportName, string entryLine)
    {
        var match = Regex.Match(
            content,
            $@"export\s+const\s+{Regex.Escape(exportName)}\s*=\s*\{{");
        if (!match.Success)
            throw new InvalidOperationException($"未找到前端权限对象：export const {exportName}");

        var open = match.Index + match.Length - 1;
        var close = FindMatchingClose(content, open);

        // 确保插入点前一项有尾逗号
        var before = content[..close].TrimEnd();
        if (!before.EndsWith('{') && !before.EndsWith(','))
            before += ",";

        var nl = content.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        return before + nl + entryLine + nl + content[close..];
    }
}
