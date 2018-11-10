function format(str, zoom, x, y) {
    return str.replace("${z}", zoom).replace("${x}", x).replace("${y}", y);
}

function switch_url(url) {
    var list = url.match(/(http:\/\/[0-9a-z\-]*?)\{switch:([a-z0-9,]+)\}(.*)/);

    if (!list || list.length == 0) {
        return url;
    }

    var servers = list[2].split(",");
    var url_list = [];
    for (var i = 0; i < servers.length; i++) {
        url_list.push(list[1] + servers[i] + list[3]);
    }

    return url_list;
}

