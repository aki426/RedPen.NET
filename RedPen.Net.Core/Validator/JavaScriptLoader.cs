using System;
using System.Collections.Generic;
using System.Text;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Tokenizer;

namespace RedPen.Net.Core.Validator
{
    // MEMO: JSのValidatorに関しては一時Pending。

    ///// <summary>
    ///// The java script loader.
    ///// </summary>
    //public class JavaScriptLoader : Validator
    //{
    //    private static Logger LOG = LogManager.GetCurrentClassLogger();
    //    private readonly string name;

    //    // TODO: JAVAのInvocableはC#のScriptに対応しそうだが、実際の動作確認は要検討。
    //    // MEMO: そもそもC#アプリにJSのValidatorを組み込むためにはJering.Javascript.NodeJSなどを使用する必要がある。
    //    //private final Invocable invocable;
    //    //private static final ScriptEngineManager manager = new ScriptEngineManager();
    //    //private readonly Script<object> script;

    //    private readonly string message;

    //    private static readonly string[] MethodsToBeExposedToJS = {
    //        "GetInt", "GetFloat", "GetString", "GetBoolean", "GetSet",
    //        "GetConfigAttribute", "GetSymbolTable", "AddError", "AddErrorWithPosition",
    //        "AddLocalizedError", "AddLocalizedErrorFromToken", "AddLocalizedErrorWithPosition"
    //    };

    //    public JavaScriptLoader(string name, string scriptCode)
    //    {
    //        this.name = name;
    //        setValidatorName(name);

    //        // MEMO: Claudeで移植時にハルシネーションを起こしたらしいので要再検討。
    //        //var options = ScriptOptions.Default
    //        //    .WithImports("System")
    //        //    .WithReferences(typeof(JavaScriptLoader).Assembly);

    //        //script = CSharpScript.Create(scriptCode, options, typeof(JavaScriptLoader));

    //        //var globals = new JavaScriptLoaderGlobals(this);
    //        //script = script.WithGlobals(globals);
    //        //message = globals.Message;

    //        message = string.Empty;
    //    }

    //    //    public override void PreValidate(Sentence sentence)
    //    //    {
    //    //        CallFunction("PreValidateSentence", sentence);
    //    //    }

    //    //    public override void PreValidate(Section section)
    //    //    {
    //    //        CallFunction("PreValidateSection", section);
    //    //    }

    //    //    public override void Validate(Document document)
    //    //    {
    //    //        CallFunction("ValidateDocument", document);
    //    //    }

    //    //    public override void Validate(Sentence sentence)
    //    //    {
    //    //        CallFunction("ValidateSentence", sentence);
    //    //    }

    //    //    public override void Validate(Section section)
    //    //    {
    //    //        CallFunction("ValidateSection", section);
    //    //    }

    //    //    private readonly Dictionary<string, bool> functionExistenceMap = new Dictionary<string, bool>();

    //    //    private void CallFunction(string functionName, params object[] args)
    //    //    {
    //    //        bool functionExists = functionExistenceMap.GetValueOrDefault(functionName, true);
    //    //        if (functionExists)
    //    //        {
    //    //            try
    //    //            {
    //    //                script.CreateDelegate(functionName).DynamicInvoke(args);
    //    //            }
    //    //            catch (Exception e)
    //    //            {
    //    //                Log.Error($"Failed to invoke {functionName}", e);
    //    //            }
    //    //            catch (Exception)
    //    //            {
    //    //                functionExistenceMap[functionName] = false;
    //    //            }
    //    //        }
    //    //    }

    //    //    public override object GetOrDefault(string name)
    //    //    {
    //    //        object value = base.GetOrDefault(this.name.Replace(".js", "") + "-" + name);
    //    //        if (value == null)
    //    //        {
    //    //            value = base.GetOrDefault(name);
    //    //        }
    //    //        return value;
    //    //    }

    //    //    protected override string GetLocalizedErrorMessage(string key, params object[] args)
    //    //    {
    //    //        if (!string.IsNullOrEmpty(message))
    //    //        {
    //    //            return string.Format(message, args);
    //    //        }
    //    //        else
    //    //        {
    //    //            return base.GetLocalizedErrorMessage(key, args);
    //    //        }
    //    //    }
    //    //}

    //    //public class JavaScriptLoaderGlobals
    //    //{
    //    //    public string Message { get; }

    //    //    public JavaScriptLoaderGlobals(JavaScriptLoader loader)
    //    //    {
    //    //        GetInt = new Func<string, int>(loader.GetInt);
    //    //        GetFloat = new Func<string, float>(loader.GetFloat);
    //    //        GetString = new Func<string, string>(loader.GetString);
    //    //        GetBoolean = new Func<string, bool>(loader.GetBoolean);
    //    //        GetSet = new Func<string, HashSet<string>>(loader.GetSet);
    //    //        GetConfigAttribute = new Func<string, string>(name => loader.GetConfigAttribute(name)?.ToString());
    //    //        GetSymbolTable = new Func<SymbolTable>(() => loader.GetSymbolTable());
    //    //        AddError = new Action<string, Sentence>(loader.AddError);
    //    //        AddErrorWithPosition = new Action<string, Sentence, int, int>(loader.AddErrorWithPosition);
    //    //        AddLocalizedError = new Action<Sentence, object[]>(
    //    //            (sentence, args) => loader.AddLocalizedError(sentence, args));
    //    //        AddLocalizedErrorFromToken = new Action<Sentence, TokenElement, object[]>(
    //    //            (sentence, token, args) => loader.AddLocalizedErrorFromToken(sentence, token, args));
    //    //        AddLocalizedErrorWithPosition = new Action<Sentence, int, int, object[]>(
    //    //            (sentence, start, end, args) => loader.AddLocalizedErrorWithPosition(sentence, start, end, args));

    //    //        Message = (string)loader.script.CreateDelegate("get_message")();
    //    //    }

    //    //    public Func<string, int> GetInt { get; }
    //    //    public Func<string, float> GetFloat { get; }
    //    //    public Func<string, string> GetString { get; }
    //    //    public Func<string, bool> GetBoolean { get; }
    //    //    public Func<string, HashSet<string>> GetSet { get; }
    //    //    public Func<string, string> GetConfigAttribute { get; }
    //    //    public Func<SymbolTable> GetSymbolTable { get; }
    //    //    public Action<string, Sentence> AddError { get; }
    //    //    public Action<string, Sentence, int, int> AddErrorWithPosition { get; }
    //    //    public Action<Sentence, object[]> AddLocalizedError { get; }
    //    //    public Action<Sentence, TokenElement, object[]> AddLocalizedErrorFromToken { get; }
    //    //    public Action<Sentence, int, int, object[]> AddLocalizedErrorWithPosition { get; }
    //    //}
    //}
}
