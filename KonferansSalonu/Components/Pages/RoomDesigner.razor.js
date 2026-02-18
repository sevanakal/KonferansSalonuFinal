window.getElementBoundingClient = (elementId) => {
    var element = document.getElementById(elementId);
    if (!element) return null;

    var rect = element.getBoundingClientRect();
    return {
        X: rect.x,
        Y: rect.y,
        Width: rect.width,
        Height: rect.height
    }
}