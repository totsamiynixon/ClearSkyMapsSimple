jQuery($ => {
    window.CSM.offlinePage = {
        template: "#offlinePageTemplate",
        data: () => ({
            offline: true
        }),
        mounted: function () {
            const that = this;
            window.addEventListener('offline', () => {
                that.offline = true;
            });
            window.addEventListener('online', () => {
                that.offline = false;
            });
        },
        methods: {
            reconnect: function () {
                this.$router.push("/");
            }
        }
    };
});