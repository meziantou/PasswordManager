export function removeChildren(parentNode: Node) {
    while (parentNode.firstChild) {
        parentNode.removeChild(parentNode.firstChild);
    }
}