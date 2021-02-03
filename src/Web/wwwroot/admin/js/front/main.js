jQuery($ => {
    const adminModule = {};
    window.CSM_Admin = {
        addModule: (name, value) => {
            adminModule[name] = value;
        },
        getModule: name => adminModule[name]
    }
})

Vue.filter('toTime', value => {
    if (value) {
        return moment(value).format('h:mm:ss');
    }
});