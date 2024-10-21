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
        qrText = "https://twitter.com/intent/tweet?text=AR%E4%BC%9A%E3%81%AE%E3%82%B2%E3%83%BC%E3%83%A0THE%20D1CE%E3%81%A7" + ScoreManager.instance.score.ToString() + "%E7%82%B9%E3%82%92%E7%8D%B2%E5%BE%97%E3%81%97%E3%81%BE%E3%81%97%E3%81%9F&url=https://xi-server.ayutaso.com/ranking/&hashtags=AR会&via=arcircle";
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
