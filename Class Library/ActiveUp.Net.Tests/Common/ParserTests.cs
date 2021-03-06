﻿using System;
using System.IO;
using ActiveUp.Net.Mail;
using NUnit.Framework;
using System.Reflection;

namespace ActiveUp.Net.Tests.Common
{
    [TestFixture]
    public class ParserTests
    {
        private static string _baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        [Test]
        public void should_parse_simple_date()
        {
            var utcDate = Parser.ParseAsUniversalDateTime("Mon, 24 Jun 2013 10:37:36 +0100");

            utcDate.ShouldEqual(new DateTime(2013, 06, 24, 09, 37, 36));
        }

        [Test]
        public void should_clean_input()
        {
            var utcDate = Parser.ParseAsUniversalDateTime("(noise\\input)Mon, 24 Jun 2013 10:37:36 +0100");

            utcDate.ShouldEqual(new DateTime(2013, 06, 24, 09, 37, 36));
        }

        [Test]
        public void should_return_resulting_date_in_utc()
        {
            var utcDate = Parser.ParseAsUniversalDateTime("Mon, 24 Jun 2013 10:37:36 +0100");

            utcDate.Kind.ShouldEqual(DateTimeKind.Utc);
        }

        [Test]
        public void should_parse_date_with_no_day_of_week()
        {
            var utcDate = Parser.ParseAsUniversalDateTime("24 Jun 2013 10:37:36 +0100");

            utcDate.ShouldEqual(new DateTime(2013, 06, 24, 09, 37, 36));
        }

        [Test]
        public void should_parse_date_with_two_digits_year()
        {
            var utcDate = Parser.ParseAsUniversalDateTime("Mon, 24 Jun 13 10:37:36 +0100");

            utcDate.ShouldEqual(new DateTime(2013, 06, 24, 09, 37, 36));
        }

        [Test]
        public void should_parse_date_with_no_seconds()
        {
            var utcDate = Parser.ParseAsUniversalDateTime("Mon, 24 Jun 2013 10:37 +0100");

            utcDate.ShouldEqual(new DateTime(2013, 06, 24, 09, 37, 00));
        }

        [Test]
        public void should_parse_basic_address()
        {
            var address = Parser.ParseAddress("here@there.com");
            address.Email.ShouldEqual("here@there.com");
            address.Name.ShouldEqual(string.Empty);
        }

        [Test]
        public void should_parse_quoted_address()
        {
            var address = Parser.ParseAddress("<\"here@there.com\">");
            address.Email.ShouldEqual("here@there.com");
            address.Name.ShouldEqual(string.Empty);
        }

        [Test]
        public void should_parse_address_with_quoted_display_name()
        {
            var address = Parser.ParseAddress("\"Display Name\" <display@name.de>");
            address.Email.ShouldEqual("display@name.de");
            address.Name.ShouldEqual("Display Name");
        }

        [Test]
        public void should_parse_address_with_non_quoted_display_name()
        {
            var address = Parser.ParseAddress("DisplayName <display@name.de>");
            address.Email.ShouldEqual("display@name.de");
            address.Name.ShouldEqual("DisplayName");
        }

        [Test]
        public void should_parse_address_with_chevrons_in_display_name()
        {
            var address = Parser.ParseAddress("\"Display Name <with Chevrons>\" <Chevrons@displayname.de>");
            address.Email.ShouldEqual("Chevrons@displayname.de");
            address.Name.ShouldEqual("Display Name <with Chevrons>");
        }

        [Test]
        public void should_parse_address_with_no_closing_quote_after_display_name()
        {
            var address = Parser.ParseAddress("\"Display Name only one quote <Chevrons@displayname.de>");
            address.Email.ShouldEqual("Chevrons@displayname.de");
            address.Name.ShouldEqual("Display Name only one quote");
        }

        [Test]
        public void should_parse_address_with_invalid_empty_quote()
        {
            var address = Parser.ParseAddress("\"\" Invoice@dymak.nl\"");
            address.Email.ShouldEqual("Invoice@dymak.nl");
            address.Name.ShouldEqual("");
        }

        /// <summary>
        /// [discussion:641270] - Created discussion to validate if this test is rigth.
        /// </summary>
        [Test]
        public void should_append_text_parts_with_inline_disposition()
        {
            var message = Parser.ParseMessageFromFile(_baseDir + "\\resource\\text_multipart_email.eml");

            message.BodyText.Text.ShouldEqual("Good morning,\r\nThis is the body of the message.\r\n\r\nThis is the attached disclamer\r\n");
        }

        /// <summary>
        /// [discussion:641270] - Created discussion to validate if this test is rigth.
        /// </summary>
        [Test]
        public void should_append_html_parts_with_inline_disposition()
        {
            var message = Parser.ParseMessageFromFile(_baseDir + "\\resource\\html_multipart_email.eml");

            message.BodyHtml.Text.ShouldEqual("Good morning,\r\n<em>This is the body of the message.</em>\r\n\r\nThis is the <em>attached</em> disclamer\r\n");
        }

        [Test]
        public void should_decode_japanese_content()
        {
            var message = Parser.ParseMessage(File.ReadAllText(_baseDir + "\\resource\\japanese_email.eml"));
            var subject = "Fwd: 大阪瓦斯 (9532) : シェール生産動向、気温が当面の焦点";
            Assert.AreEqual(subject, message.Subject);
            var textBody = "J.P. Morgan Markets Research Email Alerts\r\nhttps://jpmm.com/\r\n\r\nDear Anna-Maria Nilsson,\r\n\r\nJ.P. Morgan Research has published the following on J.P. MorganMarkets.\r\nPlease click on the indicated URL to access your document.\r\n\r\nHeadline:  大阪瓦斯 (9532)  : シェール生産動向、気温が当面の焦点\r\n\r\nAuthor(s):\r\nYuji Nishiyama\r\nhttps://jpmm.com/research/analyst/V577405\r\n\r\n\r\nAbstract:  決算の印象は芳しくなかった：\r\n第2四半期決算を受けて、業績予想を見直した。目標株価を500円から490円に引き下げるが、投資判断「Overweight」を継続する。決算および決算説明会の印象が芳しくなかったことに加え、同業他社による不定期な値下げ実施、ガス制度改革等、当面は向かい風が強く、株価は動意薄とならざるを得ないと考える。ただ、中長期的には海外投資による利益成長と、それに伴う株主還元拡充が期待でき、魅力的な投資対象であるとの見解は維持する。\r\nシェールの生産不調：\r\n決算の印象を悪化させた第1の要因は、米国のピアソール・シェールにおける生産不調に起因する海外エネルギーの通期下方修正。埋蔵量は当初想定通りとのことであり、将来の可能性が減じたわけではないが、短期的には生産が軌道に乗る見通しがまだ立っていない模様であるため、懸念材料として意識されるだろう。\r\nのれん代過多の買収： 第2の要因はJacobi\r\nCarbonsののれん代で、決算説明会では2.5億ドルであることが明らかにされた。買収予定金額は3.9億ドルであり、PBRにして2.8倍となる。営業利益対比で見ると、買収金額はその13倍と、類似取引事例などから鑑みて必ずしも割高ではないが、のれん代の規模は当社が想定していたよりも大きく、市場の評価は得にくいだろう。\r\n今週に入り、気温は急速に低下：\r\n第3四半期決算に向けての焦点は、冬場の気温とピアソール・シェールの生産動向となろう。10月の平均気温は平年を1.8度上回り、11月上旬でも0.3度上回っている。ただ、今週に入って気温が急速に低下し、11月11日の平均気温は12.4度と平年を2度も下回っており、低気温が持続すればガス事業の上振れが期待できる。\r\n\r\nLink:  https://jpmm.com/research/content/GPS-1258467-0\r\n\r\n\r\nDate:  Wed Nov 13 01:15:14 EST 2013\r\n\r\n------------------------------------------------------------------------------------\r\n\r\nVisit J.P. Morgan Markets at https://jpmm.com/research/disclosures\r\n\r\nIf you wish to change your J.P. Morgan Email Alert preferences or\r\nunsubscribe, visit:\r\nhttps://jpmm.com/research/page/cfp_my_alerts\r\n------------------------------------------------------------------------------------\r\n\r\nCopyright @ 1999, 2013 JPMorgan Chase & Co. All Rights Reserved.\r\n\r\nThis email alert is sent only to authorized J.P. Morgan clients and is for\r\ninformational purposes only. This email alert may contain hyperlinks and/or\r\nattachments to J.P. Morgan research that you requested. Additional\r\ninformation available upon request.\r\n\r\nThis email alert may not be forwarded or distributed to any other person\r\nand may not be reproduced in any manner whatsoever. It is not intended as\r\nan offer or solicitation for the purchase or sale of any financial\r\ninstrument or as an official confirmation of any transaction. Access to the\r\nresearch described herein is made available only to authorized J.P. Morgan\r\nclients with a valid ID and password to the J.P Morgan Markets website. The\r\nresearch referred to in this email alert is made available globally by J.P.\r\nMorgan Securities LLC, J.P. Morgan Futures Inc., J.P. Morgan Securities\r\nplc, J.P. Morgan plc, J.P. Morgan Europe Limited or their affiliates as\r\ndesignated via the J.P. Morgan Markets website. All market prices, data and\r\nother information are not warranted as to completeness or accuracy and are\r\nsubject to change without notice. Any comments or statements made herein do\r\nnot necessarily reflect those of JPMorgan Chase & Co., its subsidiaries and\r\naffiliates.\r\n\r\nJ.P. Morgan's Full Disclaimer:  https://jpmm.com/research/disclosures\r\n\n-- \n----------------------------------------------------------------------------------------------------\n*ABC arbitrage, partenaire officiel du skipper Jean-Pierre Dick // ABC \narbitrage, official partner of skipper Jean-Pierre Dick // www.jpdick.com \n<http://www.jpdick.com>*\nPlease consider your environmental responsibility before printing this email\n*********************************************************************************\nCe message peut contenir des informations confidentielles. Les idees et \nopinions presentees dans ce message sont celles de son auteur, et ne \nrepresentent pas necessairement celles du groupe ABC arbitrage.\nAu cas ou il ne vous serait pas destine,merci d'en aviser l'expediteur \nimmediatement et de le supprimer.\n\nThis message may contain confidential information. Any views or opinions \npresented are solely those of its author and do not necessarily represent \nthose of ABC arbitrage. \nIf you are not the intended recipient, please notify the sender immediately \nand delete it.\n*********************************************************************************\n\n";
            Assert.AreEqual(textBody, message.BodyText.Text);
            var htmlBody = "<div dir=\"ltr\"><br><div class=\"gmail_quote\"><br><br>\r\nJ.P. Morgan Markets Research Email Alerts<br>\r\n<a href=\"https://jpmm.com/\" target=\"_blank\">https://jpmm.com/</a><br>\r\n<br>\r\nDear Anna-Maria Nilsson,<br>\r\n<br>\r\nJ.P. Morgan Research has published the following on J.P. MorganMarkets. &nbsp; Please click on the indicated URL to access your document.<br>\r\n<br>\r\nHeadline: &nbsp;大阪瓦斯 (9532) &nbsp;: シェール生産動向、気温が当面の焦点<br>\r\n<br>\r\nAuthor(s):<br>\r\nYuji Nishiyama<br>\r\n<a href=\"https://jpmm.com/research/analyst/V577405\" target=\"_blank\">https://jpmm.com/research/analyst/V577405</a><br>\r\n<br>\r\n<br>\r\nAbstract: &nbsp;決算の印象は芳しくなかった： 第2四半期決算を受けて、業績予想を見直した。目標株価を500円から490円に引き下げるが、投資判断「Overweight」を継続する。決算および決算説明会の印象が芳しくなかったことに加え、同業他社による不定期な値下げ実施、ガス制度改革等、当面は向かい風が強く、株価は動意薄とならざるを得ないと考える。ただ、中長期的には海外投資による利益成長と、それに伴う株主還元拡充が期待でき、魅力的な投資対象であるとの見解は維持する。 シェールの生産不調： 決算の印象を悪化させた第1の要因は、米国のピアソール・シェールにおける生産不調に起因する海外エネルギーの通期下方修正。埋蔵量は当初想定通りとのことであり、将来の可能性が減じたわけではないが、短期的には生産が軌道に乗る見通しがまだ立っていない模様であるため、懸念材料として意識されるだろう。 のれん代過多の買収： 第2の要因はJacobi Carbonsののれん代で、決算説明会では2.5億ドルであることが明らかにされた。買収予定金額は3.9億ドルであり、PBRにして2.8倍となる。営業利益対比で見ると、買収金額はその13倍と、類似取引事例などから鑑みて必ずしも割高ではないが、のれん代の規模は当社が想定していたよりも大きく、市場の評価は得にくいだろう。 今週に入り、気温は急速に低下： 第3四半期決算に向けての焦点は、冬場の気温とピアソール・シェールの生産動向となろう。10月の平均気温は平年を1.8度上回り、11月上旬でも0.3度上回っている。ただ、今週に入って気温が急速に低下し、11月11日の平均気温は12.4度と平年を2度も下回っており、低気温が持続すればガス事業の上振れが期待できる。<br>\r\n\r\n\r\n<br>\r\nLink: &nbsp;<a href=\"https://jpmm.com/research/content/GPS-1258467-0\" target=\"_blank\">https://jpmm.com/research/content/GPS-1258467-0</a><br>\r\n<br>\r\n<br>\r\nDate: &nbsp;Wed Nov 13 01:15:14 EST 2013<br>\r\n<br>\r\n------------------------------------------------------------------------------------<br>\r\n<br>\r\nVisit J.P. Morgan Markets at <a href=\"https://jpmm.com/research/disclosures\" target=\"_blank\">https://jpmm.com/research/disclosures</a><br>\r\n<br>\r\nIf you wish to change your J.P. Morgan Email Alert preferences or unsubscribe, visit:<br>\r\n<a href=\"https://jpmm.com/research/page/cfp_my_alerts\" target=\"_blank\">https://jpmm.com/research/page/cfp_my_alerts</a><br>\r\n------------------------------------------------------------------------------------<br>\r\n<br>\r\nCopyright @ 1999, 2013 JPMorgan Chase &amp; Co. All Rights Reserved.<br>\r\n<br>\r\nThis email alert is sent only to authorized J.P. Morgan clients and is for informational purposes only. This email alert may contain hyperlinks and/or attachments to J.P. Morgan research that you requested. Additional information available upon request.<br>\r\n\r\n\r\n<br>\r\nThis email alert may not be forwarded or distributed to any other person and may not be reproduced in any manner whatsoever. It is not intended as an offer or solicitation for the purchase or sale of any financial instrument or as an official confirmation of any transaction. Access to the research described herein is made available only to authorized J.P. Morgan clients with a valid ID and password to the J.P Morgan Markets website. The research referred to in this email alert is made available globally by J.P. Morgan Securities LLC, J.P. Morgan Futures Inc., J.P. Morgan Securities plc, J.P. Morgan plc, J.P. Morgan Europe Limited or their affiliates as designated via the J.P. Morgan Markets website. All market prices, data and other information are not warranted as to completeness or accuracy and are subject to change without notice. Any comments or statements made herein do not necessarily reflect those of JPMorgan Chase &amp; Co., its subsidiaries and affiliates.<br>\r\n\r\n\r\n<br>\r\nJ.P. Morgan&#39;s Full Disclaimer: &nbsp;<a href=\"https://jpmm.com/research/disclosures\" target=\"_blank\">https://jpmm.com/research/disclosures</a><br>\r\n</div><br></div>\r\n\n<br>\n<div style=\"font-size:1.3em\"><font face=\"Arial, Helvetica, sans-serif\" size=\"2\">------------------------------<WBR>------------------------------<WBR>------------------------------<WBR>----------</font></div><div style=\"font-size:1.3em\"><font face=\"Arial\" size=\"1\"><b><i>ABC arbitrage, partenaire officiel du skipper Jean-Pierre Dick // ABC arbitrage, official partner of skipper Jean-Pierre Dick // <a href=\"http://www.jpdick.com\" target=\"_blank\">www.jpdick.com</a></i></b></font></div><div style=\"font-size:1.3em\"><font face=\"Arial\" color=\"#008000\" size=\"1\">Please consider your environmental responsibility before printing this email</font></div><div style=\"font-size:1.3em\"><font face=\"Arial, Helvetica, sans-serif\" size=\"2\">******************************<WBR>******************************<WBR>*********************</font></div><div><font size=\"1\"><font face=\"Arial, Helvetica, sans-serif\">Ce message peut contenir des informations confidentielles.&nbsp;</font><span style=\"font-family:Arial\">Les idees et opinions presentees dans ce message&nbsp;</span><span style=\"font-family:Arial\">sont celles de son auteur, et ne representent pas necessairement&nbsp;</span><span style=\"font-family:Arial\">celles du groupe ABC arbitrage.</span></font></div><div><font size=\"1\"><font face=\"Arial\">Au cas ou il ne vous serait pas destine,</font><span style=\"font-family:Arial\">merci d&#39;en aviser l&#39;expediteur immediatement et de le supprimer.</span></font></div><div><font face=\"Arial\" size=\"1\"><br></font></div><div><font size=\"1\"><font face=\"Arial\">This message may contain confidential information.&nbsp;</font><span style=\"font-family:Arial\">Any views or opinions presented are solely those of its author&nbsp;</span><span style=\"font-family:Arial\">and do not necessarily represent those of ABC arbitrage.&nbsp;</span></font></div><div><font size=\"1\"><font face=\"Arial\">If you are not the intended recipient,&nbsp;</font><span style=\"font-family:Arial\">please notify the sender immediately and delete it.</span></font></div><div style=\"font-size:1.3em\"><font face=\"Arial, Helvetica, sans-serif\" size=\"2\">******************************<WBR>******************************<WBR>*********************</font></div><div style=\"font-size:1.3em;font-family:Arial,Helvetica,sans-serif\"><br></div>";
            Assert.AreEqual(htmlBody, message.BodyHtml.Text);
            message.Attachments[0].ContentName.ShouldEqual("大阪瓦斯9532.pdf");
        }
        
        /// <summary>
        ///  https://tools.ietf.org/html/rfc2387
        /// </summary>
        [Test(Description = "LineBreak \r or \n only fail.")]
        public void should_recognize_line_break_of_notepad_text_in_body()
        {
            var message = Parser.ParseMessageFromFile(_baseDir + "\\resource\\quoted-printable-notepad-linebreak.eml");
            message.BodyText.Text.ShouldEqual("Alatur,\r\rFoi criada uma nova solicitação para TESTE SOLICITANTE.\r\rCliente: TESTE HOTEL\rEmpresa: TESTE\rC. Custo: TESTE TESTE\r\r\r>>> PASSAGEM AÉREA\rDescrição.: (GRU) Cumbica / (LAS) Las Vegas 04/Jan Manhã (06:00 às 12:00) (Econômica)\rHorário...: considerando saída\rPagamento.: FATURADO\r\rDescrição.: (LAS) Las Vegas / (GRU) Cumbica 07/Jan Manhã (06:00 às 12:00) (Econômica)\rHorário...: considerando saída\rPagamento.: FATURADO\r\r\r>>> SOLICITANTE\rteste solicitante (fulfillment@alatur.com)\r\r\rDestinatários que estão recebendo esse email: \rtms@argoit.com.br (tms@argoit.com.br)\rteste solicitante (fulfillment@alatur.com)\rtesteodare@encontact.com.br (testeodare@encontact.com.br)\rodare@encontact.com.br (odare@encontact.com.br)\r\rPara acessá-la clique em: \r<https://arb.alatur.com/alatur/default.aspx?Id=5a03cdaf-1503-e611-9406-90b11c25f027&LinkId=FLXMfbCeRo72PRAkakfyOg%3d%3d> \r\rEMAIL AUTOMÁTICO, NÃO RESPONDA ESSA MENSAGEM\r\n");
            message.BodyHtml.Text.ShouldEqual("");
        }

        /// <summary>
        /// TODO: Change test to fail process correct action in attachment without filename. Actualy the system process.
        /// </summary>
        [Test(Description = "Attachment without filename")]
        public void ParseAttachmentWitoutFilename()
        {
            Message message = Parser.ParseMessageFromFile(_baseDir + "\\resource\\attachment-witout-file-name.eml");
            message.Attachments.Count.ShouldEqual(2);
            for (int i = 0; i < message.Attachments.Count; i++)
                Assert.IsNotNull(message.Attachments[i].Filename);
        }
        /// <summary>
        /// Fields: Confirm-Reading-To, Return-Receipt-To, Disposition-Notification-To was indicated without e-mail address.
        /// RFC3798 has more information about details of parse https://tools.ietf.org/html/rfc3798
        /// NOTE: Without address the system will work with a null return on parse.
        /// </summary>
        [Test(Description = "ConfirmRead, DispositionNotificationTo and ReturnReceiptTo having exception.")]
        public void MustParseInvalidConfirmReadReturnReceipt()
        {
            Message message = Parser.ParseMessageFromFile(_baseDir + "\\resource\\confirm_read_parse_problem.eml");
            Assert.IsNull(message.ConfirmRead);
            Assert.IsNull(message.ReturnReceipt);
            Assert.AreEqual(0, message.Recipients.Count);
        }

        [Test(Description = "")]
        public void MustParseEmlWithWrongImageAsPartOfEmailBody()
        {
            var message = Parser.ParseMessageFromFile(_baseDir + "\\resource\\image-as-body-part.eml");
            Assert.AreEqual("CAM3z=h1WB0qSZPU+PWL5VqxsL1k1gmh0pmJivD1G+LjNC5jTLA@mail.serverhost.com", message.MessageId);
            Assert.AreEqual("Boa tarde roger,\r\n\r\nAgradeço a atenção e atendimento. Pode fechar o pedido com 2 cápsulas no\r\nvalor passado de $123.312.313,04.\r\n\r\nMoro em Abracox, eu busco pessoalmente ou recebo no meu endereço? E qual o\r\nprazo de entrega e formas de pagamento?\r\n\r\nObrigado,\r\nJosé roger\r\n\r\n\r\nEm 23 de julho de 2016 09:00, roger Munes <\r\nroger@destinataryhost.com> escreveu:\r\n\r\n> Com 2 cáp deu $123.312.313,04 fico no seu aguardo para finalizar o pedido..\r\n>\r\n>\r\n> Atenciosamente, roger Mussa\r\n>  [image: Customer Supplier]*roger de Souza Nunes* /\r\n> Atendimento\r\n> roger@destinataryhost.com <%7bEmail%7d>\r\n>\r\n>\r\n> *Customer supplier*\r\n> 0800 116 7284 -  (99) 9376-8104\r\n> http://www.destinataryhost.com\r\n>\r\n>\r\n>\r\n> [image: Twitter] <https://www.twitter.com/customersupplier>  [image:\r\n> Facebook] <https://www.facebook.com/custsupplier>  [image: Instagram]\r\n> <https://www.instagram.com/custsupplier>\r\n> Antes de imprimir este e-mail veja se é necessário e pense em sua\r\n> responsabilidade com o *Meio Ambiente*.\r\n>\r\n>\r\n>\r\n> *De:* rogerneto@serverhost.com\r\n> *Enviada em:* 22/07/2016 19:13:51\r\n> *Para:* roger Munes\r\n> *Assunto:* Re: Re: Contact\r\n> Olá roger, esse valor é com 90 cápsulas, correto? Veja por gentileza com\r\n> 2 aproveito para comprar logo mais.\r\n>\r\n> Obrigado pela atenção.\r\n>\r\n> José roger\r\n>\r\n> Em 22 de julho de 2016 16:05, roger Munes <\r\n> roger@destinataryhost.com> escreveu:\r\n>\r\n> Boa tarde tudo bem ? orçamento 345788 consegui por $ 2.222,00\r\n> fico no seu aguardo.\r\n>\r\n>\r\n> Atenciosamente, roger Mussa\r\n>  [image: Customer Supplier]*roger Munes* /\r\n> Atendimento\r\n> roger@destinataryhost.com <%7bEmail%7d>\r\n>\r\n>\r\n> *Customer supplier*\r\n> 0800 116 7284 -  (99) 9376-8104\r\n> http://www.destinataryhost.com\r\n>\r\n>\r\n>\r\n> [image: Twitter] <https://www.twitter.com/customersupplier>  [image:\r\n> Facebook] <https://www.facebook.com/custsupplier>  [image: Instagram]\r\n> <https://www.instagram.com/custsupplier>\r\n> Antes de imprimir este e-mail veja se é necessário e pense em sua\r\n> responsabilidade com o *Meio Ambiente*.\r\n>\r\n>\r\n>\r\n> *De:* rogerneto@serverhost.com\r\n> *Enviada em:* 22/07/2016 14:55:08\r\n> *Para:* roger Munes\r\n> *Assunto:* Re: Contact\r\n> Boa tarde roger,\r\n>\r\n> Agradeço o contato. Ainda não comprei porém tenho o orçamento abaixo que\r\n> infelizmente está abaixo da Miligrama. Caso consiga cobrir, prefiro comprar\r\n> com vocês por já ser cliente e ter outras compras com sucesso no histórico.\r\n>\r\n>\r\n> Obrigado,\r\n> José roger\r\n>\r\n>\r\n>\r\n> Em 22 de julho de 2016 14:49, roger Munes <\r\n> roger@destinataryhost.com> escreveu:\r\n>\r\n> Boa tarde amigo, como vai ?\r\n>\r\n> Chegou a finalizar o pedido, comprou em outro lugar ? que achou do meu\r\n> orçamento vamos negociar cubro a oferta de qualquer concorrente.\r\n>\r\n>\r\n> Atenciosamente, roger Mussa\r\n>  [image: Customer Supplier]*roger de Souza Nunes* /\r\n> Atendimento\r\n> roger@destinataryhost.com <%7bEmail%7d>\r\n>\r\n>\r\n> *Customer supplier*\r\n> 0800 116 7284 -  (99) 9376-8104\r\n> http://www.destinataryhost.com\r\n>\r\n>\r\n>\r\n> [image: Twitter] <https://www.twitter.com/customersupplier>  [image:\r\n> Facebook] <https://www.facebook.com/custsupplier>  [image: Instagram]\r\n> <https://www.instagram.com/custsupplier>\r\n> Antes de imprimir este e-mail veja se é necessário e pense em sua\r\n> responsabilidade com o *Meio Ambiente*.\r\n>\r\n>\r\n>\r\n>\r\n".Replace("\r\n", ""), message.BodyText.Text.Replace("\r\n", ""));
            Assert.AreEqual("<div dir=\"ltr\">Boa tarde roger,<div><br></div><div>Agradeço a atenção e atendimento. Pode fechar o pedido com 2 cápsulas no valor passado de <span style=\"font-size:12.8px\">$ 2.222,00.</span></div><div><br></div><div>Moro em Cubivila, eu busco pessoalmente ou recebo no meu endereço? E qual o prazo de entrega e formas de pagamento?</div><div><br></div><div>Obrigado,</div><div>José roger</div><div><br></div></div><div class=\"gmail_extra\"><br><div class=\"gmail_quote\">Em 23 de julho de 2016 09:00, roger Munes <span dir=\"ltr\">&lt;<a href=\"mailto:roger@destinataryhost.com\" target=\"_blank\">roger@custsupplier.com..br</a&gt;</span> escreveu:<br><blockquote class=\"gmail_quote\" style=\"margin:0 0 0 .8ex;border-left:1px #ccc solid;padding-left:1ex\"><div>Com 2 cáp deu $ 2.222,00 fico no seu aguardo para finalizar o pedido.    \r\n<span class=\"\"><div>\r\n<p><br>\r\nAtenciosamente, roger Mussa<br>\r\n <img alt=\"Customer Supplier\" src=\"http://www.customerhost.com/assinatura/logo.png\"><strong>roger de Souza Nunes</strong> / Atendimento<br>\r\n<a href=\"mailto:%7bEmail%7d\" target=\"_blank\">roger@destinataryhost.com</a><br>\r\n<br>\r\n<br>\r\n<strong>Customer supplier</strong> <br>\r\n0800 116 7284 - <img alt=\"\" src=\"http://customerhost.com/assinatura/whatsappm.png\"> (99) 9376-8104<br>\r\n<a href=\"http://www.destinataryhost.com/\" target=\"_blank\">http://www.destinataryhost.com</a>  <br>\r\n<br>\r\n<br>\r\n<br>\r\n<a href=\"https://www.twitter.com/customersupplier\" target=\"_blank\"><img alt=\"Twitter\" src=\"http://www.customerhost.com/assinatura/twitterm.png\"></a>  <a href=\"https://www.facebook.com/custsupplier\" target=\"_blank\"><img alt=\"Facebook\" src=\"http://www.customerhost.com/assinatura/facebookm.png\"></a>  <a href=\"https://www.instagram.com/custsupplier\" target=\"_blank\"><img alt=\"Instagram\" src=\"http://www.customerhost.com/assinatura/instagramm.png\"></a><br>\r\nAntes de imprimir este e-mail veja se é necessário e pense em sua responsabilidade com o <strong>Meio Ambiente</strong>.</p>\r\n\r\n<p><img alt=\"\" src=\"cid:31391411\" style=\"border:0px solid black;min-height:200px;margin-bottom:0px;margin-left:0px;margin-right:0px;margin-top:0px;width:600px\"></p>\r\n\r\n<p> </p>\r\n</div>\r\n \r\n\r\n</span><div style=\"text-align:left\"><strong>De:</strong> <a href=\"mailto:rogerneto@serverhost.com\" target=\"_blank\">rogerneto@serverhost.com</a><br>\r\n<strong>Enviada em:</strong> 22/07/2016 19:13:51<span class=\"\"><br>\r\n<strong>Para:</strong> roger Munes<br>\r\n</span><strong>Assunto:</strong> Re: Re: Contact</div><div><div class=\"h5\">\r\n\r\n<div>Olá roger, esse valor é com 90 cápsulas, correto? Veja por gentileza com 2 aproveito para comprar logo mais.\r\n<div> </div>\r\n\r\n<div>Obrigado pela atenção.</div>\r\n\r\n<div> </div>\r\n\r\n<div>José roger</div>\r\n</div>\r\n\r\n<div> \r\n<div>Em 22 de julho de 2016 16:05, roger Munes &lt;<a href=\"mailto:roger@destinataryhost.com\" target=\"_blank\">roger@destinataryhost.com</a>&gt; escreveu:\r\n\r\n<blockquote>\r\n<div>Boa tarde tudo bem ? orçamento 345788 consegui por $ 2.222,00<br>\r\nfico no seu aguardo.\r\n<div>\r\n<p><br>\r\nAtenciosamente, roger Mussa<br>\r\n <img alt=\"Customer Supplier\" src=\"http://www.customerhost.com/assinatura/logo.png\"><strong>roger de Souza Nunes</strong> / Atendimento<br>\r\n<a href=\"mailto:%7bEmail%7d\" target=\"_blank\">roger@destinataryhost.com</a><br>\r\n<br>\r\n<br>\r\n<strong>Customer supplier</strong> <br>\r\n0800 116 7284 - <img alt=\"\" src=\"http://customerhost.com/assinatura/whatsappm.png\"> (99) 9376-8104<br>\r\n<a href=\"http://www.destinataryhost.com/\" target=\"_blank\">http://www.fmiligrama.com.br</a>  <br>\r\n<br>\r\n<br>\r\n<br>\r\n<a href=\"https://www.twitter.com/customersupplier\" target=\"_blank\"><img alt=\"Twitter\" src=\"http://www.customerhost.com/assinatura/twitterm.png\"></a>  <a href=\"https://www.facebook.com/custsupplier\" target=\"_blank\"><img alt=\"Facebook\" src=\"http://www.customerhost.com/assinatura/facebookm.png\"></a>  <a href=\"https://www.instagram.com/custsupplier\" target=\"_blank\"><img alt=\"Instagram\" src=\"http://www.customerhost.com/assinatura/instagramm.png\"></a><br>\r\nAntes de imprimir este e-mail veja se é necessário e pense em sua responsabilidade com o <strong>Meio Ambiente</strong>.</p>\r\n\r\n<p><img alt=\"\" src=\"cid:16636849\" style=\"border:0px solid black;margin-bottom:0px;margin-left:0px;margin-right:0px;margin-top:0px;min-height:200px;width:600px\"></p>\r\n\r\n<p> </p>\r\n</div>\r\n \r\n\r\n<div style=\"text-align:left\"><strong>De:</strong> <a href=\"mailto:ramalhoneto@serverhost.com\" target=\"_blank\">rogerneto@serverhost.com</a><br>\r\n<strong>Enviada em:</strong> 22/07/2016 14:55:08<br>\r\n<strong>Para:</strong> roger Munes<br>\r\n<strong>Assunto:</strong> Re: Contact</div>\r\n\r\n<div>\r\n<div>\r\n<div>Boa tarde roger,\r\n<div> </div>\r\n\r\n<div>Agradeço o contato. Ainda não comprei porém tenho o orçamento abaixo que infelizmente está abaixo da Miligrama. Caso consiga cobrir, prefiro comprar com vocês por já ser cliente e ter outras compras com sucesso no histórico.</div>\r\n\r\n<div> </div>\r\n\r\n<div> </div>\r\n\r\n<div>Obrigado,</div>\r\n\r\n<div>José roger</div>\r\n\r\n<div> </div>\r\n\r\n<div> </div>\r\n\r\n<div> </div>\r\n\r\n<div>\r\n<div>Em 22 de julho de 2016 14:49, roger Munes &lt;<a href=\"mailto:roger@destinataryhost.com\" target=\"_blank\">roger@destinataryhost.com</a>&gt; escreveu:\r\n\r\n<blockquote>\r\n<div>Boa tarde amigo, como vai ?<br>\r\n<br>\r\nChegou a finalizar o pedido, comprou em outro lugar ? que achou do meu orçamento vamos negociar cubro a oferta de qualquer concorrente.\r\n<div>\r\n<p><br>\r\nAtenciosamente, roger Mussa<br>\r\n <img alt=\"Customer Supplier\" src=\"http://www.customerhost.com/assinatura/logo.png\"><strong>roger de Souza Nunes</strong> / Atendimento<br>\r\n<a href=\"mailto:%7bEmail%7d\" target=\"_blank\">roger@destinataryhost.com</a><br>\r\n<br>\r\n<br>\r\n<strong>Customer supplier</strong> <br>\r\n0800 116 7284 - <img alt=\"\" src=\"http://customerhost.com/assinatura/whatsappm.png\"> (99) 9376-8104<br>\r\n<a href=\"http://www.destinataryhost.com/\" target=\"_blank\">http://www.fmiligrama.com.br</a>  <br>\r\n<br>\r\n<br>\r\n<br>\r\n<a href=\"https://www.twitter.com/customersupplier\" target=\"_blank\"><img alt=\"Twitter\" src=\"http://www.customerhost.com/assinatura/twitterm.png\"></a>  <a href=\"https://www.facebook.com/custsupplier\" target=\"_blank\"><img alt=\"Facebook\" src=\"http://www.customerhost.cor/assinatura/facebookm.png\"></a>  <a href=\"https://www.instagram.com/custsupplier\" target=\"_blank\"><img alt=\"Instagram\" src=\"http://www.customerhost.com/assinatura/instagramm.png\"></a><br>\r\nAntes de imprimir este e-mail veja se é necessário e pense em sua responsabilidade com o <strong>Meio Ambiente</strong>.</p>\r\n\r\n<p><img alt=\"\" src=\"cid:16636849\" style=\"border:0px solid black;margin-bottom:0px;margin-left:0px;margin-right:0px;margin-top:0px;min-height:200px;width:600px\"></p>\r\n\r\n<p> </p>\r\n</div>\r\n</div>\r\n</blockquote>\r\n</div>\r\n</div>\r\n</div>\r\n</div>\r\n</div>\r\n</div>\r\n</blockquote>\r\n</div>\r\n</div>\r\n</div></div></div></blockquote></div><br></div>\r\n".Replace("\r\n", ""), message.BodyHtml.Text.Replace("\r\n", ""));
            Assert.AreEqual("Re: Re: Re: Contact", message.Subject);
            Assert.AreEqual(1, message.To.Count);
            Assert.AreEqual(2, message.EmbeddedObjects.Count);
            Assert.AreEqual(0, message.Attachments.Count);

            // Manual validation: Save the attachment to validate if has valid image.
            //var i = 0;
            //foreach (MimePart item in message.EmbeddedObjects)
            //{
            //    var fileName = item.ContentName ?? "file" + i + ".jpg";
            //    var fileNameDecoded = item.ContentName ?? "file_decoded_" + i + ".jpg";
            //    i++;
            //    File.WriteAllBytes(fileName, item.BinaryContent);
            //    File.WriteAllBytes(fileNameDecoded, Convert.FromBase64String(item.TextContentTransferEncoded));
            //}
        }

        [Test(Description = "")]
        public void MustParseEmlWithoutContentTypeSubtypeWithLostTextBody()
        {
            var message = Parser.ParseMessageFromFile(_baseDir + "\\resource\\text_without_contenttype_subtype.eml");
            Assert.AreEqual("hash@sender.production.server.com", message.MessageId);
            Assert.IsFalse(string.IsNullOrWhiteSpace(message.BodyText.Text));
            Assert.IsTrue(string.IsNullOrWhiteSpace(message.BodyHtml.Text));
            Assert.AreEqual("plain", message.ContentType.SubType);
            Assert.AreEqual("text", message.ContentType.Type);
            Assert.AreEqual("text", message.ContentType.MimeType);
        }

        [Test(Description = "")]
        public void MustParseEmlWithContentTransferEncode8Bit()
        {
            var message = Parser.ParseMessageFromFile(_baseDir + "\\resource\\content-transfer-encode-8bit.eml");
            Assert.AreEqual("58caaa74.6625ed0a.22a2d.5376@mx.google.com", message.MessageId);
            Assert.AreEqual("Special char test  çãõáéíóú", message.Subject);
            Assert.IsFalse(string.IsNullOrWhiteSpace(message.BodyText.Text));
            Assert.AreEqual("Special char test  çãõáéíóú", message.BodyText.Text);
            Assert.IsTrue(string.IsNullOrWhiteSpace(message.BodyHtml.Text));
        }
    }
}
