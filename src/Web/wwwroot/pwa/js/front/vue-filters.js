jQuery($ => {
    //filters
    Vue.filter('toTime', value => {
        if (value) {
            return moment(value).format('h:mm:ss');
        }
    });
});