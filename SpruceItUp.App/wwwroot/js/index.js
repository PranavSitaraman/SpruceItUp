var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var PinKind;
(function (PinKind) {
    PinKind[PinKind["Litter"] = 0] = "Litter";
    PinKind[PinKind["Park"] = 1] = "Park";
    PinKind[PinKind["Trail"] = 2] = "Trail";
    PinKind[PinKind["Other"] = 3] = "Other";
})(PinKind || (PinKind = {}));
let map, meMarker;
let pinMarkers = {};
let pins = {};
let mousedUp;
let mapNetObject;
let meIcon;
let markerIconsLoc;
let markerIconsPin;
let PinPopup;
const meSize = 50;
const pinIconSize = 40;
function getLocation(netObject) {
    function returnPosition(position) {
        return __awaiter(this, void 0, void 0, function* () {
            yield netObject.invokeMethodAsync('SetLocation', position.coords.latitude, position.coords.longitude);
        });
    }
    function positionError(error) { }
    if (navigator.geolocation) {
        navigator.geolocation.watchPosition(returnPosition, positionError);
        return true;
    }
    else {
        return false;
    }
}
function initMap(netObject, elementId, lat, lon, zoom) {
    initResources();
    mapNetObject = netObject;
    let latLng = new google.maps.LatLng(lat, lon);
    let options = {
        zoom: zoom,
        center: latLng,
        mapTypeId: google.maps.MapTypeId.ROADMAP,
        streetViewControl: false,
        scaleControl: false,
        zoomControl: false,
        mapTypeControl: false,
        fullscreenControl: false,
        clickableIcons: false
    };
    let mapElement = document.getElementById(elementId);
    map = new google.maps.Map(mapElement, options);
    pinMarkers = {};
    pins = {};
    mousedUp = false;
    map.addListener('mousedown', (event) => {
        mousedUp = false;
        setTimeout(() => __awaiter(this, void 0, void 0, function* () {
            if (mousedUp === false) {
                let lat = event.latLng.lat();
                let lon = event.latLng.lng();
                yield netObject.invokeMethodAsync("OnLongPress", lat, lon);
            }
        }), 500);
    });
    map.addListener('mouseup', () => mousedUp = true);
    map.addListener('dragstart', () => mousedUp = true);
}
function initResources() {
    meIcon =
        {
            url: "/assets/icons/man.svg",
            scaledSize: new google.maps.Size(meSize, meSize),
            origin: new google.maps.Point(0, 0),
            anchor: new google.maps.Point(meSize / 2, meSize / 2)
        };
    markerIconsPin =
        {
            [PinKind.Litter]: {
                url: '/assets/icons/litter1.svg',
                scaledSize: new google.maps.Size(pinIconSize, pinIconSize),
                origin: new google.maps.Point(0, 0),
                anchor: new google.maps.Point(pinIconSize / 2, pinIconSize / 2)
            },
            [PinKind.Park]: {
                url: '/assets/icons/park1.svg',
                scaledSize: new google.maps.Size(pinIconSize, pinIconSize),
                origin: new google.maps.Point(0, 0),
                anchor: new google.maps.Point(pinIconSize / 2, pinIconSize / 2)
            },
            [PinKind.Trail]: {
                url: "/assets/icons/trail1.svg",
                scaledSize: new google.maps.Size(pinIconSize, pinIconSize),
                origin: new google.maps.Point(0, 0),
                anchor: new google.maps.Point(pinIconSize / 2, pinIconSize / 2)
            },
            [PinKind.Other]: {
                url: "/assets/icons/other1.svg",
                scaledSize: new google.maps.Size(pinIconSize, pinIconSize),
                origin: new google.maps.Point(0, 0),
                anchor: new google.maps.Point(pinIconSize / 2, pinIconSize / 2)
            }
        };
    markerIconsLoc =
        {
            [PinKind.Litter]: {
                url: '/assets/icons/litter2.svg',
                scaledSize: new google.maps.Size(pinIconSize, pinIconSize),
                origin: new google.maps.Point(0, 0),
                anchor: new google.maps.Point(pinIconSize / 2, pinIconSize / 2)
            },
            [PinKind.Park]: {
                url: '/assets/icons/park2.svg',
                scaledSize: new google.maps.Size(pinIconSize, pinIconSize),
                origin: new google.maps.Point(0, 0),
                anchor: new google.maps.Point(pinIconSize / 2, pinIconSize / 2)
            },
            [PinKind.Trail]: {
                url: "/assets/icons/trail2.svg",
                scaledSize: new google.maps.Size(pinIconSize, pinIconSize),
                origin: new google.maps.Point(0, 0),
                anchor: new google.maps.Point(pinIconSize / 2, pinIconSize / 2)
            },
            [PinKind.Other]: {
                url: "/assets/icons/other2.svg",
                scaledSize: new google.maps.Size(pinIconSize, pinIconSize),
                origin: new google.maps.Point(0, 0),
                anchor: new google.maps.Point(pinIconSize / 2, pinIconSize / 2)
            }
        };
    PinPopup = class extends google.maps.OverlayView {
        constructor(marker, content) {
            super();
            this.marker = marker;
            this.position = marker.getPosition();
            let closeContainer = document.createElement('div');
            closeContainer.classList.add("popup-bubble");
            const closeIcon = document.createElement("ion-icon");
            closeIcon["name"] = "close";
            closeIcon.onclick = () => { this.setMap(null); };
            closeContainer.appendChild(content);
            closeContainer.appendChild(closeIcon);
            closeContainer.setAttribute('style', 'border: thin solid white;');
            const bubbleAnchor = document.createElement("div");
            bubbleAnchor.classList.add("popup-bubble-anchor");
            bubbleAnchor.appendChild(closeContainer);
            this.containerDiv = document.createElement("div");
            this.containerDiv.classList.add("popup-container");
            this.containerDiv.appendChild(bubbleAnchor);
            PinPopup.preventMapHitsAndGesturesFrom(this.containerDiv);
        }
        onAdd() {
            this.getPanes().floatPane.appendChild(this.containerDiv);
        }
        onRemove() {
            if (this.containerDiv.parentElement) {
                this.containerDiv.parentElement.removeChild(this.containerDiv);
            }
        }
        draw() {
            const divPosition = this.getProjection().fromLatLngToDivPixel(this.position);
            const display = "block";
            if (display === "block") {
                this.containerDiv.style.left = divPosition.x + "px";
                this.containerDiv.style.top = divPosition.y + "px";
            }
            if (this.containerDiv.style.display !== display) {
                this.containerDiv.style.display = display;
            }
        }
    };
}
let locMarker = {};
let locSet = {};
let locBorder = {};
function locDetails(id) {
    mapNetObject.invokeMethodAsync('ViewLocDetails', id);
}
function previewLoc(id) {
    let loc = locSet[id];
    let marker = locMarker[id];
    let content = document.createElement("div");
    content.className = "google-map-info";
    content.innerHTML = `<h2>${loc.title}</h2>`;
    content.onclick = () => locDetails(id);
    curInfoWindowId = id;
    curInfoWindow = new PinPopup(marker, content);
    curInfoWindow.setMap(map);
}
function placeLoc(loc) {
    locSet[loc.id] = loc;
    let avgx = 0.0, avgy = 0.0;
    for (var i = 0; i < loc.lat.length; i++) {
        avgx += loc.lat[i];
        avgy += loc.lng[i];
    }
    avgx /= loc.lat.length;
    avgy /= loc.lat.length;
    let marker = new google.maps.Marker({
        position: { lat: avgx, lng: avgy },
        map: map,
        icon: markerIconsLoc[loc.kind]
    });
    marker.addListener("click", () => previewLoc(loc.id));
    locMarker[loc.id] = marker;
    let border = [];
    for (var j = 0; j < loc.lat.length; j++)
        border.push(new google.maps.LatLng(loc.lat[j], loc.lng[j]));
    let location = new google.maps.Polygon({
        paths: border,
        strokeColor: "#0000FF",
        strokeOpacity: 0.8,
        strokeWeight: 3,
        fillColor: "#0000FF",
        fillOpacity: 0.35,
    });
    location.setMap(map);
    location.addListener('click', () => previewLoc(loc.id));
    locBorder[loc.id] = location;
}
function moveMeMarker(lat, lon) {
    let pos = { lat: lat, lng: lon };
    if (meMarker == undefined) {
        meMarker = new google.maps.Marker({
            position: pos,
            map: map,
            icon: meIcon
        });
    }
    else {
        meMarker.setPosition(pos);
    }
}
let curInfoWindow;
let curInfoWindowId;
function previewPin(id) {
    let pin = pins[id];
    let marker = pinMarkers[id];
    let content = document.createElement("div");
    content.className = "google-map-info";
    content.innerHTML = `<h2>${pin.title}</h2>`;
    content.onclick = () => pinDetails(id);
    curInfoWindowId = id;
    curInfoWindow = new PinPopup(marker, content);
    curInfoWindow.setMap(map);
}
let locs = {};
var numlocs = 0;
function orientation(p, q, r) {
    let val = (q[1] - p[1]) * (r[0] - q[0]) - (q[0] - p[0]) * (r[1] - q[1]);
    if (val == 0)
        return 0;
    return (val > 0) ? 1 : 2;
}
function convexHull(points, n) {
    let hull = [];
    let l = 0;
    for (let i = 1; i < n; i++)
        if (points[i][0] < points[l][0])
            l = i;
    let p = l, q;
    do {
        hull.push(points[p]);
        q = (p + 1) % n;
        for (let i = 0; i < n; i++) {
            if (orientation(points[p], points[i], points[q]) == 2)
                q = i;
        }
        p = q;
    } while (p != l);
    return hull;
}
function locSelect(paths, ids) {
    mapNetObject.invokeMethodAsync('SelectLoc', paths, ids);
}
function createLoc(near) {
    var data = JSON.parse(near);
    if (data == null)
        return;
    for (var i = 0; i < numlocs; i++)
        locs[i].setMap(null);
    google.maps.event.clearInstanceListeners(google.maps.Polygon);
    locs = {};
    numlocs = data.length;
    for (var i = 0; i < data.length; i++) {
        let parsed = [];
        let ids = [];
        for (var j = 0; j < data[i].length; j++) {
            parsed.push([data[i][j].Lat, data[i][j].Lon]);
            ids.push(data[i][j].Id);
        }
        let hull = convexHull(parsed, data[i].length);
        let convexhull = [];
        for (var j = 0; j < hull.length; j++)
            convexhull.push(new google.maps.LatLng(hull[j][0], hull[j][1]));
        let location = new google.maps.Polygon({
            paths: convexhull,
            strokeColor: "#FF0000",
            strokeOpacity: 0.8,
            strokeWeight: 3,
            fillColor: "#FF0000",
            fillOpacity: 0.35,
        });
        locs[i] = location;
        location.setMap(map);
        location.addListener('click', () => locSelect(hull, ids));
    }
}
function placePin(pin) {
    pins[pin.id] = pin;
    let marker = new google.maps.Marker({
        position: { lat: pin.lat, lng: pin.lon },
        map: map,
        icon: markerIconsPin[pin.kind]
    });
    marker.addListener("click", () => previewPin(pin.id));
    pinMarkers[pin.id] = marker;
}
function pinDetails(id) {
    mapNetObject.invokeMethodAsync('ViewDetails', id);
}
let longPressIndicator;
function placeLongPressIndicator(lat, lon) {
    let pos = { lat: lat, lng: lon };
    if (longPressIndicator == undefined) {
        longPressIndicator = new google.maps.Marker({
            position: pos,
            map: map,
            icon: {
                url: "/assets/icons/pin.svg",
                scaledSize: new google.maps.Size(meSize, meSize),
                origin: new google.maps.Point(0, 0),
                anchor: new google.maps.Point(meSize / 2, meSize),
                fillColor: "red"
            }
        });
    }
    else {
        longPressIndicator.setPosition(pos);
    }
}
function removeLongPressIndicator() {
    if (longPressIndicator != undefined) {
        longPressIndicator.setMap(null);
        longPressIndicator = undefined;
    }
}
//# sourceMappingURL=index.js.map