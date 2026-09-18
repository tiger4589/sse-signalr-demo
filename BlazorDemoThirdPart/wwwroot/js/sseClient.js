const connections = {};

export function connect(id, url, dotNetObject, messageCallback, errorCallback) {
    disconnect(id);

    const source = new EventSource(url);
    let opened = false;

    return new Promise((resolve, reject) => {
        source.onopen = () => {
            opened = true;
            resolve();
        };

        source.onmessage = (event) => {
            dotNetObject.invokeMethodAsync(messageCallback, id, event.data);
        };

        source.onerror = () => {
            dotNetObject.invokeMethodAsync(errorCallback, id, `Connection failed or was closed for ${url}.`);
            disconnect(id);

            if (!opened) {
                reject(new Error(`SSE connection failed before opening for ${url}.`));
            }
        };

        connections[id] = source;
    });
}

export function disconnect(id) {
    const source = connections[id];
    if (!source) {
        return;
    }

    source.close();
    delete connections[id];
}
