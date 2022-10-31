import DotNetObject = DotNet.DotNetObject;
export interface Pin
{
    id: string;
    author: string;
    title: string;
    lat: number;
    lon: number;
    kind: PinKind;
    expires: string;
    created: string;
    image: string | null;
    description: string | null;
}
export interface Loc {
    id: string;
    author: string;
    title: string;
    lat: number[];
    lng: number[];
    kind: PinKind;
    expires: string;
    created: string;
    image: string | null;
    description: string | null;
    pins: string[];
}
enum PinKind
{
    Litter,
    Park,
    Trail,
    Other
}
interface Position
{
    lat: number;
    lon: number;
}
let map, meMarker;
let pinMarkers: Record<string, google.maps.Marker> = {};
let pins: Record<string, Pin> = {};
let mousedUp: boolean;
let mapNetObject: DotNetObject;
let meIcon: google.maps.Icon;
let markerIconsLoc: Record<PinKind, google.maps.Icon>;
let markerIconsPin: Record<PinKind, google.maps.Icon>;
let PinPopup;
const meSize = 50;
const pinIconSize = 40;
function getLocation(netObject: DotNetObject): boolean
{
    async function returnPosition(position: GeolocationPosition)
    {
        await netObject.invokeMethodAsync('SetLocation', position.coords.latitude, position.coords.longitude);
    }
    function positionError(error: GeolocationPositionError) { }
    if (navigator.geolocation)
    {
        navigator.geolocation.watchPosition(returnPosition, positionError);
        return true;
    }
    else
    {
        return false;
    }
}
function initMap(netObject: DotNetObject, elementId: string, lat: number, lon: number, zoom: number)
{
    initResources();
    mapNetObject = netObject;
    let latLng = new google.maps.LatLng(lat, lon);
    let options: google.maps.MapOptions =
    {
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
    map.addListener('mousedown', (event) =>
    {
        mousedUp = false;
        setTimeout(async () =>
        {
            if (mousedUp === false)
            {
                let lat = event.latLng.lat();
                let lon = event.latLng.lng();
                await netObject.invokeMethodAsync("OnLongPress", lat, lon);
            }
        }, 500);
    });
    map.addListener('mouseup', () => mousedUp = true);
    map.addListener('dragstart', () => mousedUp = true);
}
function initResources()
{
    meIcon =
    {
        url: "/assets/icons/man.svg",
        scaledSize: new google.maps.Size(meSize, meSize),
        origin: new google.maps.Point(0, 0),
        anchor: new google.maps.Point(meSize / 2, meSize / 2)
    };
    markerIconsPin =
    {
        [PinKind.Litter]:
        {
            url: '/assets/icons/litter1.svg',
            scaledSize: new google.maps.Size(pinIconSize, pinIconSize),
            origin: new google.maps.Point(0, 0),
            anchor: new google.maps.Point(pinIconSize / 2, pinIconSize / 2)
        },
        [PinKind.Park]:
        {
            url: '/assets/icons/park1.svg',
            scaledSize: new google.maps.Size(pinIconSize, pinIconSize),
            origin: new google.maps.Point(0, 0),
            anchor: new google.maps.Point(pinIconSize / 2, pinIconSize / 2)
        },
        [PinKind.Trail]:
        {
            url: "/assets/icons/trail1.svg",
            scaledSize: new google.maps.Size(pinIconSize, pinIconSize),
            origin: new google.maps.Point(0, 0),
            anchor: new google.maps.Point(pinIconSize / 2, pinIconSize / 2)

        },
        [PinKind.Other]:
        {
            url: "/assets/icons/other1.svg",
            scaledSize: new google.maps.Size(pinIconSize, pinIconSize),
            origin: new google.maps.Point(0, 0),
            anchor: new google.maps.Point(pinIconSize / 2, pinIconSize / 2)
        }
    };
    markerIconsLoc =
    {
        [PinKind.Litter]:
        {
            url: '/assets/icons/litter2.svg',
            scaledSize: new google.maps.Size(pinIconSize, pinIconSize),
            origin: new google.maps.Point(0, 0),
            anchor: new google.maps.Point(pinIconSize / 2, pinIconSize / 2)
        },
        [PinKind.Park]:
        {
            url: '/assets/icons/park2.svg',
            scaledSize: new google.maps.Size(pinIconSize, pinIconSize),
            origin: new google.maps.Point(0, 0),
            anchor: new google.maps.Point(pinIconSize / 2, pinIconSize / 2)
        },
        [PinKind.Trail]:
        {
            url: "/assets/icons/trail2.svg",
            scaledSize: new google.maps.Size(pinIconSize, pinIconSize),
            origin: new google.maps.Point(0, 0),
            anchor: new google.maps.Point(pinIconSize / 2, pinIconSize / 2)

        },
        [PinKind.Other]:
        {
            url: "/assets/icons/other2.svg",
            scaledSize: new google.maps.Size(pinIconSize, pinIconSize),
            origin: new google.maps.Point(0, 0),
            anchor: new google.maps.Point(pinIconSize / 2, pinIconSize / 2)
        }
    };
    PinPopup = class extends google.maps.OverlayView
    {
        marker: google.maps.Marker;
        position: google.maps.LatLng;
        containerDiv: HTMLDivElement;
        constructor(marker: google.maps.Marker, content: HTMLElement)
        {
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
        onAdd()
        {
            this.getPanes()!.floatPane.appendChild(this.containerDiv);
        }
        onRemove()
        {
            if (this.containerDiv.parentElement)
            {
                this.containerDiv.parentElement.removeChild(this.containerDiv);
            }
        }
        draw()
        {
            const divPosition = this.getProjection().fromLatLngToDivPixel(this.position)!;
            const display = "block";
            if (display === "block")
            {
                this.containerDiv.style.left = divPosition.x + "px";
                this.containerDiv.style.top = divPosition.y + "px";
            }
            if (this.containerDiv.style.display !== display)
            {
                this.containerDiv.style.display = display;
            }
        }
    }
}
let locMarker: Record<string, google.maps.Marker> = {};
let locSet: Record<string, Loc> = {};
let locBorder: Record<string, google.maps.Polygon> = {};
function locDetails(id: string)
{
    mapNetObject.invokeMethodAsync('ViewLocDetails', id);
}
function previewLoc(id: string)
{
    let loc: Loc = locSet[id];
    let marker: google.maps.Marker = locMarker[id];
    let content = document.createElement("div");
    content.className = "google-map-info";
    content.innerHTML = `<h2>${loc.title}</h2>`;
    content.onclick = () => locDetails(id);
    curInfoWindowId = id;
    curInfoWindow = new PinPopup(marker, content);
    curInfoWindow.setMap(map);
}
function placeLoc(loc: Loc)
{
    locSet[loc.id] = loc;
    let avgx = 0.0, avgy = 0.0;
    for (var i = 0; i < loc.lat.length; i++)
    {
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
    for (var j = 0; j < loc.lat.length; j++) border.push(new google.maps.LatLng(loc.lat[j], loc.lng[j]));
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
function moveMeMarker(lat: number, lon: number)
{
    let pos = {lat: lat, lng: lon};
    if (meMarker == undefined)
    {
        meMarker = new google.maps.Marker({
            position: pos,
            map: map,
            icon: meIcon
        })
    }
    else
    {
        meMarker.setPosition(pos);
    }
}
let curInfoWindow;
let curInfoWindowId;
function previewPin(id: string)
{
    let pin: Pin = pins[id];
    let marker: google.maps.Marker = pinMarkers[id];
    let content = document.createElement("div");
    content.className = "google-map-info";
    content.innerHTML = `<h2>${pin.title}</h2>`;
    content.onclick = () => pinDetails(id);
    curInfoWindowId = id;
    curInfoWindow = new PinPopup(marker, content);
    curInfoWindow.setMap(map);
}
let locs: Record<number, google.maps.Polygon> = {};
var numlocs = 0;
function orientation(p, q, r)
{
    let val = (q[1] - p[1]) * (r[0] - q[0]) - (q[0] - p[0]) * (r[1] - q[1]);
    if (val == 0) return 0;
    return (val > 0) ? 1 : 2;
}
function convexHull(points, n)
{
    let hull = [];
    let l = 0;
    for (let i = 1; i < n; i++)
        if (points[i][0] < points[l][0])
            l = i;
    let p = l, q;
    do
    {
        hull.push(points[p]);
        q = (p + 1) % n;
        for (let i = 0; i < n; i++)
        {
            if (orientation(points[p], points[i], points[q]) == 2) q = i;
        }
        p = q;
    } while (p != l);
    return hull;
}
function locSelect(paths: number[][], ids: string[])
{
    mapNetObject.invokeMethodAsync('SelectLoc', paths, ids);
}
function createLoc(near: string)
{
    var data = JSON.parse(near);
    if (data == null) return;
    for (var i = 0; i < numlocs; i++) locs[i].setMap(null);
    google.maps.event.clearInstanceListeners(google.maps.Polygon);
    locs = {};
    numlocs = data.length;
    for (var i = 0; i < data.length; i++)
    {
        let parsed = [];
        let ids = [];
        for (var j = 0; j < data[i].length; j++) {
            parsed.push([data[i][j].Lat, data[i][j].Lon]);
            ids.push(data[i][j].Id);
        }
        let hull = convexHull(parsed, data[i].length);
        let convexhull = [];
        for (var j = 0; j < hull.length; j++) convexhull.push(new google.maps.LatLng(hull[j][0], hull[j][1]));
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
function placePin(pin: Pin)
{
    pins[pin.id] = pin;
    let marker = new google.maps.Marker({
        position: { lat: pin.lat, lng: pin.lon },
        map: map,
        icon: markerIconsPin[pin.kind]
    });
    marker.addListener("click", () => previewPin(pin.id));
    pinMarkers[pin.id] = marker;
}
function pinDetails(id: string)
{
    mapNetObject.invokeMethodAsync('ViewDetails', id);
}
let longPressIndicator: google.maps.Marker;
function placeLongPressIndicator(lat: number, lon: number)
{
    let pos = {lat: lat, lng: lon};
    if (longPressIndicator == undefined)
    {
        longPressIndicator = new google.maps.Marker({
            position: pos,
            map: map,
            icon:
            {
                url: "/assets/icons/pin.svg",
                scaledSize: new google.maps.Size(meSize, meSize),
                origin: new google.maps.Point(0, 0),
                anchor: new google.maps.Point(meSize / 2, meSize),
                fillColor: "red"
            }
        })
    }
    else
    {
        longPressIndicator.setPosition(pos);
    }
}
function removeLongPressIndicator()
{
    if (longPressIndicator != undefined)
    {
        longPressIndicator.setMap(null);
        longPressIndicator = undefined;
    }
}