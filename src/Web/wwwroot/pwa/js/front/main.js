jQuery($ => {
    window.CSM = {
        askForPermissioToReceiveNotifications: () => new Promise((resolve, reject) => {
            const messaging = firebase.messaging();
            messaging.requestPermission().then(() => {
                messaging.getToken().then(token => {
                    console.log('token do usuário:', token);
                    resolve(token);
                }).catch(reject);
                ;
            }).catch(error => {
                console.error(error);
                reject();
            });
        })
    }
});

