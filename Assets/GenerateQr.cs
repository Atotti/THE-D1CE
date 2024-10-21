using UnityEngine;
using UnityEngine.UI;

public class GenerateQr : MonoBehaviour
{
    public RawImage QRcodeImage; // RawImageオブジェクトを使用

    private Texture2D EncodedQRTextire;
    private int QrTxtureW = 256;
    private int QrTxtureH = 256;
    private string qrText = "https://xi-server.ayutaso.com/ranking/";

    void Start()
    {
        string baseText = "AR会のゲームTHE D1CEで" + ScoreManager.instance.score.ToString() + "点を獲得しました";
        string encodedText = UnityEngine.Networking.UnityWebRequest.EscapeURL(baseText); // 日本語テキストをエンコード

        string qrText = "https://twitter.com/intent/tweet?text=" + encodedText + "&url=https://xi-server.ayutaso.com/ranking/&hashtags=AR会&via=arcircle";

        // QRコード生成処理
        EncodedQRTextire = new Texture2D(QrTxtureW, QrTxtureH);
        var color32 = Encode(qrText, EncodedQRTextire.width, EncodedQRTextire.height);
        EncodedQRTextire.SetPixels32(color32);
        EncodedQRTextire.Apply();

        // RawImageにテクスチャを設定
        QRcodeImage.texture = EncodedQRTextire;
    }

    private static Color32[] Encode(string textForEncoding, int width, int height)
    {
        var writer = new ZXing.BarcodeWriter
        {
            Format = ZXing.BarcodeFormat.QR_CODE,
            Options = new ZXing.QrCode.QrCodeEncodingOptions
            {
                Height = height,
                Width = width
            }
        };
        return writer.Write(textForEncoding);
    }
}
