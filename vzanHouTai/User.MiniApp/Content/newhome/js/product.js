; (function () {
    var vm = new Vue({
        el: "#page-content",
        data: {

            singlepage: [{
                title: "��ҳ��",
                description: "���޵�ҳ���ǵ��޿Ƽ��Ļ����汾����������������ǿ��ĵ�ҳ�棬֧���ֲ�ͼ����Ƶ���������֣��Զ�����������֧��������ק�����ӻ�������ơ�������������ҵ��ͨ������չʾ���ܣ����Զ�λ����λ�ã�Ϊ���̴����������������̼�������Ƭ�����ѡ��",
                bg_img: "/Content/newhome/image/page_bg.png",
                list_1: {
                    title: "�ֲ�ͼ",
                    img: "/Content/newhome/image/log-banner.png",
                    content: "banner��һ�ֹ��չʾ��ʽ��bannerһ���Ƿ���С����Ķ���λ�ã����û����С������Ϣ��ͬʱ�������û����ڽ�����Ϣ�Ĺ�ע",
                    bg_showImg: "/Content/newhome/image/banner_show.png"
                },
                list_2: {
                    title: "���ı��ı�",
                    img: "/Content/newhome/image/logo-richtxt.png",
                    content: "�ı�����˵�������໥�佻���Ĺ��ߣ�С�������Ҫ��ϢҲ�����ı�Ϊ�����ı���׼ȷ�ı����Ϣ�����ݺͺ��塣���ı�֧�ָ�ʽ��ͼ�ģ���Ϣ���أ��Ű�������С�����ı��ܾ���������",
                    bg_showImg: "/Content/newhome/image/rich-show.png"
                },
                list_3: {
                    title: "ͼƬ",
                    img: "/Content/newhome/image/img_bg.png",
                    content: "ͼ����С�����о����ṩ��Ϣ��չʾ����������ҳ�����С������ط�����Ҫ����",
                    bg_showImg: "/Content/newhome/image/img_bg.png"
                },
                list_4: {
                    title: "�Զ����",
                    img: "/Content/newhome/image/logo-form.png",
                    content: "��ҵ�û�����ͨ���Զ�����������Լ���ʵ�������������ҵ�����ã������и��Ի�����ҵ�����ơ�����ʵ�ָ�����ҵ������ԤԼ���󡣻�֧�ֵ������������̼ҹ��������ݡ�",
                    bg_showImg: "/Content/newhome/image/form-show.png"
                },







            }],

            more: [
                { img: "/Content/newhome/image/logo-video.png", title: "��Ƶչʾ", content_1: "��Ƶ��Ŀǰ��������Ȥ�����ݴ����ֶ�", content_2: "ͨ����Ƶ������ȫ��չʾ��ҵ���̼ҵ���ɫ����", content_3: "���������˽���" },
                { img: "/Content/newhome/image/logo-music.png", title: "��������", content_1: "���־��м����������Ⱦ������д�������", content_2: "�������С������������С��������ͬ", content_3: "���һ�µ����֣���Ȼ�ͻ��ͣ��פ��" },
                { img: "/Content/newhome/image/logo-map.logo.png", title: "��ͼ����", content_1: "�����û�ֱ���ҵ��̼һ���ҵ��Ӫҵ��ַ", content_2: "���Ե����ֻ���ĵ���", content_3: "����Ӧ��ͬ���ڵ��û�ϰ��" }
            ],

            show: [
                { img: "/Content/newhome/image/logo_pro1.png", title: "Ӫ���Ƕȣ�����Ӫ��", content_1: "����������Ա�ۿ�", content_2: "��͵�����Ʒ����", content_3: "�˿ͻ�������Ա��ֵ" },
                {
                    img: "/Content/newhome/image/logo_pro2.png", title: "����Ƕȣ�����Ч��", content_1: "���ٿ�̨������Ԥ��", content_2: "���ٷ�̨��ɨ����", content_3: "������̨������֧�����ƴ�ӡ������СƱ"
                },
                {
                    img: "/Content/newhome/image/logo_pro3.png", title: "����������ת���ɽ���", content_1: "�����Ѽ���41���������", content_2: "�����нӣ����ԶԽӹ��ں�", content_3: "������Ӫ�������ݷ�����׼Ӫ��"
                }
            ],
            manager: [
                { img: "/Content/newhome/image/pro_sys.png", title: "������Աϵͳ", content_1: "�ͳɱ�������Ա����", content_2: "�Զ����Ա�ȼ�����Ʒ�ۿ�", content_3: "�ٽ��˿ͻ���" },
                {
                    img: "/Content/newhome/image/pro_active.png", title: "����Ӫ���", content_1: "��ԱӪ�������̼�����ʵ�ֶೡ��", content_2: "����󣬶����͵�Ӫ��", content_3: "��������Ӫ��Ч��"
                },
                {
                    img: "/Content/newhome/image/pro_eat.png", title: "���ϵͳ", content_1: "�ɵ��޲������̨���ɷ�����ά��", content_2: "�ṩ���̼�����������", content_3: "���������ߵ���ɨ�����ܱ�ݵ㵥�ķ���"
                },
                {
                    img: "/Content/newhome/image/pro_in.png", title: "���Ӵ�ӡ��", content_1: "��ͽ�����", content_2: "�����ǰ̨����СƱ", content_3: "������ߵ㵥Ч��"
                },
            ],
            eclist_1: [
                { img: "/Content/newhome/image/logo_nav.png", title: "ͼƬ����", context: "���������۵�ͼ�����Ʒ�������ർ����֧��ͼ��������Զ��塣�����������ҵ�������Ʒ", },
                { img: "/Content/newhome/image/logo_ecbanner.png", title: "�ֲ�ͼ", context: "banner��һ�ֹ��չʾ��ʽ��bannerһ���Ƿ���С����Ķ���λ�ã����û����С������Ϣ��ͬʱ�������û����ڽ�����Ϣ�Ĺ�ע", }
            ],
            eclist_2: [
                { img: "/Content/newhome/image/logo_spec1.png", title: "����������", context: "֧�ֶ��������ã�չʾ��Ʒ��������߸�����������ϲ������Ʒ����Ʒ������ѡ����������ʹ�ø��ӱ��", },
                { img: "/Content/newhome/image/logo_spec2.png", title: "��Ʒ����", context: "֧�ֶ༶���࣬����༶����֧���Զ������򣬸����̼ҵ���Ӫ�ͻ��Ҫ���ö���Ʒ����ȷ�ƹ���Ʒ", }
            ],
            moreFunc: [
                { img: "/Content/newhome/image/logo_mess.png", title: "��Ϣģ��", context: " �������߽�����Ϣ���ͣ����ݶ�����״̬�Զ�������Ϣģ����˿�΢�����ӻ���Ƶ��" },
                {
                    img: "/Content/newhome/image/logo_vip.png", title: "��Ա����ϵͳ", context: " �Խ�΢�ſ�����֧�ֻ�Ա����ֵ���ۿ���ƷSKU������Ա�ȼ�Ȩ����������Ԫ�����֧���Զ��廹��ֱ����ת��С���򣬹��ںţ�������"
                },
                { img: "/Content/newhome/image/logo_kan.png", title: "����", context: " �����Ѿ���Ϊ�����̳Ǳز����ٵ�Ӫ���ֶΡ������õͼ���������ʵ�û���ͬʱҲ�ﵽ�����۵�����" },
                { img: "/Content/newhome/image/logo_da.png", title: "�����ƴ�ӡ��", context: " �µ��������������СƱ��������߶�������Ч�ʡ�" },
                { img: "/Content/newhome/image/logo_map.png", title: "����5�����Զ�չʾ", context: " �൱�ڶ���Ͷ�Ź�浽��Բ5������û�΢����" },
            ],

            basis: [
                { img: "/Content/newhome/image/logo_basis_1.png", title: "���̹���", context_1: "չʾӪҵִ�ա��ŵ껷����Ƭ", context_2: " ���ṩ�ķ���Ӫҵʱ�䡢��ַ����Ϣ" },
                { img: "/Content/newhome/image/logo_basis_2.png", title: "����չʾ����׼����", context_1: "��������ά�ޱ���һ�㡰�ͽ�ԭ��", context_2: " ʹ�õ���4s��С���򣬿����ڷ�Բ5������ֻ�", context_3: "���Զ����֣������̼��ҵ���׼�ͻ�" },
                {
                    img: "/Content/newhome/image/logo_basis_3.png", title: "��̬�������ߵ���", context_1: "�Զ��嶯̬��", context_2: " �ܶԳɽ��ͻ����н׶��Ե���", context_3: "�����̻������ܽ�滮"
                },
                {
                    img: "/Content/newhome/image/logo_basis_4.png", title: "��Ա�����˿ͻ���", context_1: "֧�ֻ�Ա��ֵ����Ա���ۿ�", context_2: " ��Ա������΢�ſ���", context_3: "ֱ����ת��С������û�ʹ�ø�����"
                },
                {
                    img: "/Content/newhome/image/logo_basis_5.png", title: "���߿ͷ����������", context_1: "֧�����߿ͷ�����", context_2: " �ͻ�����С��������ʱ�õ�������", context_3: "��һ����߷�������"
                },
                {
                    img: "/Content/newhome/image/logo_basis_6.png", title: "��Ϣ������Ѷ����", context_1: "�ṩ�����ѻ�ȡ��Ϣ�ĺ�ȥ��", context_2: " �̼�Ҳ���԰�4s������»", context_3: "��Ʒ���·��������"
                },
                {
                    img: "/Content/newhome/image/logo_basis_7.png", title: "���֧����������", context_1: "�ṩ�������̳ǣ�����4s��ķ�����Ŀ", context_2: " �ͻ�����ֱ��ʹ��΢�Ž���֧��", context_3: "���ӵ���Ӫ��"
                },
                {
                    img: "/Content/newhome/image/logo_basis_8.png", title: "���ݻ���", context_1: "����С������û�������", context_2: " Ϊ�̼��ṩ�����С������û�����", context_3: "ͨ���û����������ʷ�������������", context_4: "��׼Ӫ������Ӫ���Ӵ��������ƶ��ŵ귢չ"
                },
            ]




        }
    })
})()
