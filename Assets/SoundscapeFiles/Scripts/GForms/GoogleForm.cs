using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Linq;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public class GoogleForm
{
    private static string responseURL = "https://docs.google.com/forms/d/e/[FORMID]/formResponse";

    public static void submitForm(FormAnswersHolder answers, string viewFormID)
    {
        string url = responseURL.Replace("[FORMID]", viewFormID);
        GFormPOSTRequest(url, answers.GetAnswerList(), answers.pageCount);
    }

    async static void GFormPOSTRequest(string url, List<KeyValuePair<string, string>> entries, int pageCount)
    {
        WWWForm GFormSend = new WWWForm();
        GFormSend.AddField("pageHistory", string.Join(",", Enumerable.Range(0, pageCount)));

        foreach (KeyValuePair<string, string> entry in entries)
            GFormSend.AddField(entry.Key, entry.Value);

        using (UnityWebRequest www = UnityWebRequest.Post(url, GFormSend))
        {
            await www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
                Debug.LogError("GForm submission failed: " + www.error);
            else
                Debug.Log("Form successfully submitted to Google Forms!");
        }
    }

    // --- Form structure fetching ---

    public async static Task<Form> FetchForm(string questionnaireJson)
    {
        JObject creds = JObject.Parse(questionnaireJson);
        string clientEmail = creds["client_email"]!.ToString();
        string privateKeyPem = creds["private_key"]!.ToString();
        string tokenUri = creds["token_uri"]?.ToString() ?? "https://oauth2.googleapis.com/token";
        string editFormId = creds["formId"]!.ToString();

        string accessToken = await GetAccessToken(clientEmail, privateKeyPem, tokenUri);
        string apiResponseJson = await GetFormData(editFormId, accessToken);
        Debug.Log($"[GoogleForm]: Questionnaire Form JSON Response: {apiResponseJson}");
        return ParseFormResponse(editFormId, apiResponseJson);
    }

    static async Task<string> GetAccessToken(string clientEmail, string privateKeyPem, string tokenUri)
    {
        string jwt = BuildJwt(clientEmail, privateKeyPem, tokenUri);

        string body = "grant_type=" + UnityWebRequest.EscapeURL("urn:ietf:params:oauth:grant-type:jwt-bearer")
                    + "&assertion=" + UnityWebRequest.EscapeURL(jwt);

        using UnityWebRequest www = new UnityWebRequest(tokenUri, "POST");
        www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
        await www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
            throw new Exception("Token exchange failed: " + www.error);

        return JObject.Parse(www.downloadHandler.text)["access_token"]!.ToString();
    }

    static async Task<string> GetFormData(string editFormId, string accessToken)
    {
        using UnityWebRequest www = UnityWebRequest.Get("https://forms.googleapis.com/v1/forms/" + editFormId);
        www.SetRequestHeader("Authorization", "Bearer " + accessToken);
        await www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
            throw new Exception("Forms API request failed: " + www.error);

        return www.downloadHandler.text;
    }

    static string BuildJwt(string clientEmail, string privateKeyPem, string tokenUri)
    {
        string header = Base64UrlEncode(Encoding.UTF8.GetBytes("{\"alg\":\"RS256\",\"typ\":\"JWT\"}"));

        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        string payloadJson =
            $"{{\"iss\":\"{clientEmail}\"," +
            $"\"scope\":\"https://www.googleapis.com/auth/forms.body.readonly\"," +
            $"\"aud\":\"{tokenUri}\"," +
            $"\"iat\":{now}," +
            $"\"exp\":{now + 3600}}}";
        string payload = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));

        string signingInput = header + "." + payload;

        string keyBody = privateKeyPem
            .Replace("-----BEGIN PRIVATE KEY-----", "")
            .Replace("-----END PRIVATE KEY-----", "")
            .Replace("\n", "").Replace("\r", "").Trim();
        byte[] keyBytes = Convert.FromBase64String(keyBody);

        byte[] signature;
        using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
        {
            rsa.ImportParameters(ParsePkcs8RsaKey(keyBytes));
            using SHA256 sha = SHA256.Create();
            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(signingInput));
            var formatter = new RSAPKCS1SignatureFormatter(rsa);
            formatter.SetHashAlgorithm("SHA256");
            signature = formatter.CreateSignature(hash);
        }

        return signingInput + "." + Base64UrlEncode(signature);
    }

    static RSAParameters ParsePkcs8RsaKey(byte[] pkcs8)
    {
        // PKCS#8: SEQUENCE { INTEGER(ver), SEQUENCE(alg), OCTET STRING(pkcs1) }
        int pos = 0;
        EnterDerSequence(pkcs8, ref pos);
        SkipDerElement(pkcs8, ref pos);      // version
        SkipDerElement(pkcs8, ref pos);      // algorithm SEQUENCE
        byte[] pkcs1 = ReadDerOctetString(pkcs8, ref pos);

        // PKCS#1: SEQUENCE { INTEGER(ver), n, e, d, p, q, dp, dq, qi }
        pos = 0;
        EnterDerSequence(pkcs1, ref pos);
        SkipDerElement(pkcs1, ref pos);      // version
        return new RSAParameters
        {
            Modulus  = TrimLeadingZero(ReadDerIntegerBytes(pkcs1, ref pos)),
            Exponent = TrimLeadingZero(ReadDerIntegerBytes(pkcs1, ref pos)),
            D        = TrimLeadingZero(ReadDerIntegerBytes(pkcs1, ref pos)),
            P        = TrimLeadingZero(ReadDerIntegerBytes(pkcs1, ref pos)),
            Q        = TrimLeadingZero(ReadDerIntegerBytes(pkcs1, ref pos)),
            DP       = TrimLeadingZero(ReadDerIntegerBytes(pkcs1, ref pos)),
            DQ       = TrimLeadingZero(ReadDerIntegerBytes(pkcs1, ref pos)),
            InverseQ = TrimLeadingZero(ReadDerIntegerBytes(pkcs1, ref pos))
        };
    }

    static void EnterDerSequence(byte[] d, ref int pos)
    {
        if (d[pos++] != 0x30) throw new Exception("DER: expected SEQUENCE");
        ReadDerLength(d, ref pos);
    }

    static void SkipDerElement(byte[] d, ref int pos)
    {
        pos++;                            // skip tag
        int len = ReadDerLength(d, ref pos);
        pos += len;                       // skip content
    }

    static byte[] ReadDerOctetString(byte[] d, ref int pos)
    {
        if (d[pos++] != 0x04) throw new Exception("DER: expected OCTET STRING");
        int len = ReadDerLength(d, ref pos);
        byte[] val = new byte[len];
        Array.Copy(d, pos, val, 0, len);
        pos += len;
        return val;
    }

    static byte[] ReadDerIntegerBytes(byte[] d, ref int pos)
    {
        if (d[pos++] != 0x02) throw new Exception("DER: expected INTEGER");
        int len = ReadDerLength(d, ref pos);
        byte[] val = new byte[len];
        Array.Copy(d, pos, val, 0, len);
        pos += len;
        return val;
    }

    static int ReadDerLength(byte[] d, ref int pos)
    {
        int b = d[pos++];
        if (b < 0x80) return b;
        int numBytes = b & 0x7F;
        int len = 0;
        for (int i = 0; i < numBytes; i++)
            len = (len << 8) | d[pos++];
        return len;
    }

    static byte[] TrimLeadingZero(byte[] d)
    {
        if (d.Length > 1 && d[0] == 0x00)
        {
            byte[] t = new byte[d.Length - 1];
            Array.Copy(d, 1, t, 0, t.Length);
            return t;
        }
        return d;
    }

    static Form ParseFormResponse(string editFormId, string apiResponseJson)
    {
        JObject data = JObject.Parse(apiResponseJson);

        string uri = data["responderUri"]?.ToString() ?? "";
        string viewId = uri
            .Replace("https://docs.google.com/forms/d/e/", "")
            .Replace("/viewform", "");

        Form form = new Form
        {
            formTitle   = data["info"]?["title"]?.ToString() ?? "",
            description = data["info"]?["description"]?.ToString() ?? "",
            formViewId  = viewId,
            formEditId  = editFormId
        };

        JArray items = data["items"] as JArray ?? new JArray();
        foreach (JToken item in items)
        {
            if (item["questionItem"] != null)
            {
                FormElement el = ParseQuestionItem(item);
                if (el != null) form.questions.Add(el);
            }
            else if (item["questionGroupItem"] != null)
                form.questions.AddRange(ParseQuestionGroupItem(item));
            else if (item["imageItem"] != null)
                form.questions.Add(ParseImageItem(item));
            else if (item["textItem"] != null || item["pageBreakItem"] != null)
                form.questions.Add(ParseLayoutItem(item));
        }

        return form;
    }

    static FormElement ParseQuestionItem(JToken item)
    {
        JToken question = item["questionItem"]?["question"];
        if (question == null) return null;

        string qId     = "entry." + Convert.ToInt64(question["questionId"]?.ToString() ?? "0", 16);
        string qName   = item["title"]?.ToString() ?? "Untitled Question";
        bool required  = question["required"]?.Value<bool>() ?? false;
        JToken choice  = question["choiceQuestion"];
        string qType;

        if (question["textQuestion"] != null)
            qType = question["textQuestion"]["paragraph"] != null ? FormElement.PARAGRAPH : FormElement.TEXT;
        else if (question["scaleQuestion"] != null)
            qType = FormElement.SCALE;
        else if (choice != null)
            qType = choice["type"]?.ToString() ?? FormElement.RADIO;
        else
            return null;

        List<string> extra = new();
        bool shuffle = false;

        if (choice != null)
        {
            foreach (JToken opt in choice["options"] as JArray ?? new JArray())
                extra.Add(opt["isOther"]?.Value<bool>() == true ? "Outro (Texto Livre)" : opt["value"]?.ToString() ?? "__Other__");
            shuffle = choice["shuffle"]?.Value<bool>() ?? false;
        }
        else if (qType == FormElement.SCALE)
        {
            JToken scaler = question["scaleQuestion"];
            extra.Add(scaler["low"]?.ToString()  ?? "0");
            extra.Add(scaler["high"]?.ToString() ?? "10");
            string lowLabel  = scaler["lowLabel"]?.ToString();
            string highLabel = scaler["highLabel"]?.ToString();
            if (lowLabel != null || highLabel != null)
            {
                extra.Add(lowLabel  ?? "");
                extra.Add(highLabel ?? "");
            }
        }

        return new FormElement { id = qId, type = qType, required = required, description = qName, extra = extra, shuffle = shuffle };
    }

    static List<FormElement> ParseQuestionGroupItem(JToken item)
    {
        var elements   = new List<FormElement>();
        JToken group   = item["questionGroupItem"];
        string title   = item["title"]?.ToString() ?? "Untitled Grid";
        JToken cols    = group["grid"]?["columns"];
        bool shuffle   = cols?["shuffle"]?.Value<bool>() ?? false;
        string qType   = cols?["type"]?.ToString() ?? "RADIO";

        var colOptions = new List<string>();
        foreach (JToken opt in cols?["options"] as JArray ?? new JArray())
            colOptions.Add(opt["value"]?.ToString() ?? "");

        foreach (JToken rowQ in group["questions"] as JArray ?? new JArray())
        {
            string qId     = "entry." + Convert.ToInt64(rowQ["questionId"]?.ToString() ?? "0", 16);
            string rowTitle = rowQ["rowQuestion"]?["title"]?.ToString() ?? "Untitled Row";
            bool required  = rowQ["required"]?.Value<bool>() ?? false;

            List<string> extra = new List<string> { rowTitle };
            extra.AddRange(colOptions);

            elements.Add(new FormElement
            {
                id          = qId,
                type        = $"GRID_{qType}",
                required    = required,
                description = title,
                extra       = extra,
                shuffle     = shuffle
            });
        }

        return elements;
    }

    static FormElement ParseImageItem(JToken item) => new FormElement
    {
        id          = item["itemId"]?.ToString() ?? "",
        type        = FormElement.IMAGE,
        required    = false,
        description = item["title"]?.ToString() ?? "Untitled Image",
        extra       = new List<string> { item["imageItem"]?["image"]?["contentUri"]?.ToString() ?? "" }
    };

    static FormElement ParseLayoutItem(JToken item) => new FormElement
    {
        id          = item["title"]?.ToString() ?? "",
        type        = item["textItem"] != null ? FormElement.HEADER : FormElement.SECTION_BREAK,
        required    = false,
        description = item["description"]?.ToString() ?? "",
        extra       = new List<string>()
    };

    static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
