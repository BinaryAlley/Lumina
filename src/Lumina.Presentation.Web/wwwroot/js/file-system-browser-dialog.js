//+======================================================================================+
//|                                  DOM Elements                                        |
//+======================================================================================+ 

const modalBackground = document.getElementById('modal-background');
const fileSystemBrowserDialog = document.getElementById('file-system-browser-dialog');
const fileSystemBrowserTreeview = document.getElementById('file-system-browser-treeview');
const pathSegmentsContainer = document.getElementById('navigator-path-segments');

const fileSystemTreeViewContainer = document.getElementById('file-system-browser-file-system-treeview-container');
const fileSystemExplorerContainer = document.getElementById('file-system-browser-file-system-explorer-container');
const fileSystemExplorer = document.getElementById('file-system-browser-file-system-explorer');
const fileSystemDetailsHeader = document.getElementById('file-system-browser-file-system-explorer-details-header');

const addressBarInput = document.getElementById('navigator-address-bar-input');
const addressBar = document.getElementById('navigator-address-bar');

const btnNavigatorNavigateBack = document.getElementById('navigator-navigate-back');
const btnNavigatorNavigateForward = document.getElementById('navigator-navigate-forward');
const btnNavigatorNavigateUp = document.getElementById('navigator-navigate-up');

const btnNavigatorToggleTreeView = document.getElementById('navigator-toggle-tree-view-button');
const btnNavigatorToggleSelectionMode = document.getElementById('navigator-toggle-selection-button');
const btnNavigatorHiddenElements = document.getElementById('navigator-toggle-hidden-button');
const btnEditPath = document.getElementById('navigator-edit-path-button');

const svgTreeView = document.querySelectorAll('.navigator-icon-fill.tree-view');
const svgSelectionMode = document.querySelectorAll('.navigator-icon-fill.selection-mode');
const svgHiddenElements = document.querySelectorAll('.navigator-icon-fill.hidden-elements');
const svgListView = document.querySelectorAll('.navigator-icon-fill.list');
const svgDetailsView = document.querySelectorAll('.navigator-icon-fill.details');
const svgSmallIconsView = document.querySelectorAll('.navigator-icon-fill.small-icons');
const svgMediumIconsView = document.querySelectorAll('.navigator-icon-fill.medium-icons');
const svgLargeIconsView = document.querySelectorAll('.navigator-icon-fill.large-icons');
const svgExtraLargeIconsView = document.querySelectorAll('.navigator-icon-fill.extra-large-icons');

const btnListView = document.getElementById('navigator-list-view');
const btnDetailsView = document.getElementById('navigator-details-view');
const btnSmallIconsView = document.getElementById('navigator-small-icons-view');
const btnMediumIconsView = document.getElementById('navigator-medium-icons-view');
const btnLargeIconsView = document.getElementById('navigator-large-icons-view');
const btnExtraLargeIconsView = document.getElementById('navigator-extra-large-icons-view');

const btnFileSystemBrowserEditPath = document.getElementById('navigator-edit-path-button');
const btnFileSystemBrowserSubmitPath = document.getElementById('navigator-navigate-button');
const btnFileSystemBrowserConfirm = document.getElementById('file-system-browser-confirm-button');

const selectedFileSystemElementsInput = document.getElementById('file-system-browser-selected-element');

const selectionRectangle = document.getElementById('selection-rectangle');
const visibleSelectionRectangle = document.getElementById('visible-selection-rectangle');

//+======================================================================================+
//|                                   Variables                                          |
//+======================================================================================+ 

// current icon pack theme
const CURRENT_ICON_THEME = "Lyra";
// mapping of file extensions to SVG image paths
const FILE_ICONS = {
    "ai": "application-postscript.svg",
    "apk": "android-package-archive.svg",
    "appimage": "application-vnd.appimage.svg",
    "atom": "application-atom+xml.svg",
    "avif": "application-image.svg",
    "bmp": "application-image.svg",
    "bz": "application-x-7z-ace.svg",
    "chm": "application-vnd.ms-htmlhelp.svg",
    "dicom": "application-dicom.svg",
    "dll": "application-octet-stream.svg",
    "dot": "application-msword-template.svg",
    "doc": "application-msword-template.svg",
    "docx": "application-msword-template.svg",
    "eps": "application-postscript.svg",
    "epub": "application-epub+zip.svg",
    "exe": "application-octet-stream.svg",
    "flac": "application-ogg.svg",
    "gif": "application-image.svg",
    "gz": "application-x-7z-ace.svg",
    "ico": "application-image.svg",
    "infopath": "application-vnd.ms-infopath.svg",
    "jpeg": "application-image.svg",
    "jpg": "application-image.svg",
    "json": "application-json.svg",
    "mdb": "application-vnd.ms-access.svg",
    "mp3": "application-ogg.svg",
    "ogg": "application-ogg.svg",
    "otp": "application-vnd.oasis.opendocument.presentation-template.svg",
    "ods": "application-vnd.oasis.opendocument.spreadsheet-template.svg",
    "ots": "application-vnd.oasis.opendocument.spreadsheet-template.svg",
    "ott": "application-vnd.oasis.opendocument.text-template.svg",
    "otw": "application-vnd.oasis.opendocument.web-template.svg",
    "p7s": "application-pgp-signature.svg",
    "pdf": "application-pdf.svg",
    "pict": "application-image.svg",
    "pgp": "application-pgp.svg",
    "png": "application-image.svg",
    "ppt": "application-vnd.ms-powerpoint.svg",
    "ps": "application-postscript.svg",
    "psd": "application-photoshop.svg",
    "pub": "application-vnd.ms-publisher.svg",
    "rar": "application-x-7z-ace.svg",
    "ref": "application-vnd.flatpak.ref.svg",
    "sendfile": "application-vnd.kde.bluedevil-sendfile.svg",
    "snap": "application-vnd.snap.svg",
    "stream": "application-octet-stream.svg",
    "svg": "application-image.svg",
    "sql": "application-sql.svg",
    "sqlite": "application-sql.svg",
    "tar": "application-x-7z-ace.svg",
    "tga": "application-image.svg",
    "tiff": "application-image.svg",
    "txt": "application-pgp-signature.svg",
    "wav": "application-ogg.svg",
    "webp": "application-image.svg",
    "xdgapp": "application-vnd.xdgapp.svg",
    "xls": "application-vnd.ms-excel.svg",
    "xlsx": "application-vnd.ms-excel.svg",
    "xml": "application-atom+xml.svg",
    "zip": "application-x-7z-ace.svg"
};

let serverBasePath;
let clientBasePath;
let _path;
let pathSeparator = '/';
let platformType = 'Unix';
let showFileSystemTreeView = true; // TODO: take from config
let showHiddenElements = true; // TODO: take from config
let includeFiles = true; // TODO: take from config
let viewMode = 'list-icons'; // TODO: take from config
let undoStack = [];
let redoStack = [];

let selectionStartPosition = { x: 0, y: 0 };
let currentMousePosition = { x: 0, y: 0 };
let isSelecting = false;
let isSelectionMode = false;

let scrollTimeout; // timeout to delay thumbnail loading after scrolling
let resizeTimeout; // timeout to delay thumbnail loading after resizing
let abortController; // allows the cancellation of thumbnail retrieval jobs
let hasScrolledAfterModeChange = false; // flag to track if scrolling occurred after a mode change
let showThumbnails = true; // whether to show thumbnails or not
let scrollThumbnailRetrievalTimeout = 1000; // the time span to wait after the last scroll, before getting thumbnails
let inspectFileForThumbnails = false; // whether to check the actual header bytes in order to determine if a file is an image, rather than its extension
let thumbnailsRetrievalBatchSize = 20; // the number of thumbnails to ask from the server, concurrently
let imagePreviewsQuality = 70; // the quality used for image thumbnails
let fullImageQuality = 90; // the quality used for full images


//+======================================================================================+
//|                                 Initialization                                       |
//+======================================================================================+ 

/**
 * Initializes the values needed by the file sytem browser.
 * @param {any} serverBasePathValue The base path of the server API.
 * @param {any} clientBasePathValue The base path of the client app API.
 * @param {any} initialPath The initial path of the file system browser.
 * @param {any} viewModeValue The initial view mode of he file system browser.
 * @param {any} iconSizeValue The initial icon size of the file system browser.
 */
async function initFileSystemBrowser(serverBasePathValue, clientBasePathValue, initialPath, viewModeValue, iconSizeValue) {
    serverBasePath = serverBasePathValue;
    clientBasePath = clientBasePathValue;
    await getFileSystemPropertiesAsync();
    if (!initialPath.endsWith(pathSeparator))
        initialPath += pathSeparator;

    _path = initialPath;
    addressBarInput.value = initialPath;
    showFileSystemTreeView = true; // TODO: take from config

    addHorizontalScrolling(addressBar);
    //switchViewMode('list', null, true);
    switchViewMode(viewModeValue, viewModeValue === 'list' ? null : iconSizeValue, true);
    
    // TODO: to be removed:
    showFileSystemBrowserDialogAsync(initialPath, true);
}

/**
 * Shows the file system browser dialog.
 */
async function showFileSystemBrowserDialogAsync(path, showHiddenElementsValue) {
    if (path !== undefined && path !== null) {
        showHiddenElements = showHiddenElementsValue;
        modalBackground.style.display = 'block';
        fileSystemBrowserDialog.style.display = 'block';
        if (!path.endsWith(pathSeparator))
            path += pathSeparator;
        addressBarInput.value = path;
        await toggleShowFileSystemHiddenElementsAsync(false);
        await toggleFileSystemTreeViewAsync(false);
        await navigateToPathAsync(path, false, false);
    } else
        notificationService.Show("Path cannot be null!", NotificationType.ERROR); // TODO: should ask error message from server when translation is implemented
}

/**
 * Hides the file system browser dialog.
 */
function hideFileSystemBrowserDialog() {
    modalBackground.style.display = 'block';
    fileSystemBrowserDialog.style.display = 'block';
}

/**
 * Retrieves file system properties from the server, asynchronously.
 * @returns {Promise<boolean>} A promise that resolves once the properties have been fetched.
 */
async function getFileSystemPropertiesAsync() {
    // call the API to get the file system type (platform type)
    const fileSystemTypeResponse = await callApiGetAsync(`${clientBasePath}file-system/get-type`);
    if (fileSystemTypeResponse !== undefined)
        platformType = fileSystemTypeResponse.platformType;
    // call the API to get the path separator for the file system
    const pathSeparatorResponse = await callApiGetAsync(`${clientBasePath}path/get-path-separator`);
    if (pathSeparatorResponse !== undefined)
        pathSeparator = pathSeparatorResponse.pathSeparator;
}

/**
 * Checks if the specified path is a valid file system path, and exists.
 * @param {any} path The path to validate and check.
 * @returns {Promise<boolean>} A promise that resolves to whether the path is valid and exists, or not.
 */
async function checkPathAsync(path) {
    const pathValidResponse = await callApiGetAsync(`${clientBasePath}path/validate?path=${encodeURIComponent(path)}`);
    if (pathValidResponse !== undefined) {
        if (!pathValidResponse.isValid) {
            notificationService.show("Specified path is not valid!", NotificationType.ERROR); // TODO: should ask error message from server when translation is implemented
            addressBarInput.value = _path;
            if (addressBarInput.value !== null && !addressBarInput.value.endsWith(pathSeparator))
                addressBarInput.value += pathSeparator;
        } else {
            const pathExistsResponse = await callApiGetAsync(`${clientBasePath}path/check-path-exists?path=${encodeURIComponent(path)}`);
            if (pathExistsResponse !== undefined && !pathExistsResponse.exists) {
                notificationService.show("Specified path does not exist!", NotificationType.ERROR); // TODO: should ask error message from server when translation is implemented
                return false;
            }
        }
        return pathValidResponse.isValid;
    }
}

/**
 * Navigates to the specified path,
 * @param {any} path The path o navigate to.
 * @param {any} isForwardNavigation Whether the Forward button was clicked, and navigation is done to a history location or not.
 * @param {any} isBackwardNavigation Whether the Back button was clicked, and navigation is done to a history location or not.
 */
async function navigateToPathAsync(path, isForwardNavigation, isBackwardNavigation) {
    if (await checkPathAsync(path)) {
        // if there was a job for thumbnails retrieval, cancel it first
        if (abortController) {
            abortController.abort();
            abortController = null;
        }
        // check if backward navigation is possible
        if (isBackwardNavigation) {
            if (undoStack.length > 0) {
                // get the first path in the backward navigation list
                const backwardPath = undoStack.pop();
                // put the current path in the forward navigationlist, before navigating to the backward path
                if (addressBarInput.value !== null) {
                    if (!addressBarInput.value.endsWith(pathSeparator))
                        addressBarInput.value += pathSeparator;
                    redoStack.push(addressBarInput.value);
                }
                path = backwardPath;
            } else
                return;
        } else if (isForwardNavigation) { // check if forward navigation is possible
            if (redoStack.length > 0) {
                // get the first path in the forward navigation list
                const forwardPath = redoStack.pop();
                // put the current path in the backward navigation list, before navigating to the forward path
                if (addressBarInput.value !== null) {
                    if (!addressBarInput.value.endsWith(pathSeparator))
                        addressBarInput.value += pathSeparator;
                    if (undoStack.length === 0 || (undoStack.length > 0 && undoStack[undoStack.length - 1] !== addressBarInput.value))
                        undoStack.push(addressBarInput.value);
                }
                path = forwardPath;
            } else
                return;
        } else { // navigation to a brand new location (up, directory browse, manual path edit, etc)
            redoStack = [];
            if (addressBarInput.value !== null && addressBarInput.value !== path) { // when navigating up, addressBarInput.value will be set after navigation
                if (undoStack.length === 0 || (undoStack.length > 0 && undoStack[undoStack.length - 1] !== addressBarInput.value))
                    undoStack.push(addressBarInput.value);
            } else if (_path !== null && _path !== path) { // when editing the address, addressBarInput.value already comes set, check against control value
                if (undoStack.length === 0 || (undoStack.length > 0 && undoStack[undoStack.length - 1] !== _path))
                    undoStack.push(_path);
            }
        }
        _path = path; // we are about to navigate to the new path, store its value in the control variable
        addressBarInput.style.display = 'none'; // hide #address-bar-input
        addressBar.style.display = 'block'; // show #address-bar
        await getNavigatorPathAsync(path);
        await getFileSystemItemsAsync(path);
        if (showFileSystemTreeView)
            await getFileSystemTreeViewAsync(path);
        getFileSystemVisibleItems();
    }
    addressBarInput.value = _path;
    if (addressBarInput.value !== null && !addressBarInput.value.endsWith(pathSeparator))
        addressBarInput.value += pathSeparator;
}

/**
 * Navigates up one level from the current path
 */
async function navigateUpAsync() {
    if (addressBarInput.value !== null) {
        // ask the API for the parent path
        const parentLocationResponse = await callApiGetAsync(`${clientBasePath}path/get-path-parent?path=${encodeURIComponent(addressBarInput.value)}`);
        if (parentLocationResponse !== undefined && parentLocationResponse.pathSegments !== null) {
            // if the answer was valid, and we can navigate up, reconstruct the provided path from its segments, and navigate to it
            platformType = parentLocationResponse.pathSegments;
            let parentConcatenatedPath = parentLocationResponse.pathSegments[0].path;
            for (var i = 1; i < parentLocationResponse.pathSegments.length; i++)
                parentConcatenatedPath = (parentConcatenatedPath !== pathSeparator ? parentConcatenatedPath : '') + pathSeparator + parentLocationResponse.pathSegments[i].path;
            if (!parentConcatenatedPath.endsWith(pathSeparator))
                parentConcatenatedPath += pathSeparator;
            await navigateToPathAsync(parentConcatenatedPath, false, false);
        }
    }
}


//+======================================================================================+
//|                                   Navigator                                          |
//+======================================================================================+ 

/**
 * Gets the navigator path segments.
 * @param {any} path The path for which to get the navigator path segments.
 */
async function getNavigatorPathAsync(path) {
    const pathSplitResponse = await callApiGetAsync(`${clientBasePath}path/split?path=${encodeURIComponent(path)}`);
    if (pathSplitResponse !== undefined) 
        renderAddressBar(pathSplitResponse.pathSegments);
}

/**
 * Render the address bar based on the provided path segments.
 * @param {Array<PathSegmentEntity>} pathSegments - An array of path segments.
 */
function renderAddressBar(pathSegments) {
    pathSegmentsContainer.removeEventListener('change', handlePathSegmentComboboxChangeAsync);  // remove previous listeners
    pathSegmentsContainer.addEventListener('change', handlePathSegmentComboboxChangeAsync);  // add new listener
    pathSegmentsContainer.innerHTML = ""; // clear existing segments
    if (pathSegments !== null) {
        pathSegments.forEach((segment, index) => {
            const li = document.createElement("li");
            li.id = `path-segment-${index}`;
            // create the combobox
            const combobox = document.createElement("div");
            combobox.className = "navigator-combobox inline-block";
            // the shine effect 
            const shineEffect = document.createElement("div");
            shineEffect.className = "shine-effect";
            shineEffect.style.top = "1px";
            combobox.appendChild(shineEffect);
            const toggleCheckbox = document.createElement("input");
            toggleCheckbox.type = "checkbox";
            toggleCheckbox.className = "navigator-toggle-checkbox";
            toggleCheckbox.id = `segment-toggle-${index}`;
            combobox.appendChild(toggleCheckbox);
            const toggleLabel = document.createElement("label");
            toggleLabel.className = "navigator-toggle";
            toggleLabel.htmlFor = `segment-toggle-${index}`;
            const span = document.createElement("span");
            span.className = "navigator-selected-text";
            span.innerText = segment.path;
            toggleLabel.appendChild(span);
            // add the arrow element
            const arrowSpan = document.createElement("span");
            arrowSpan.className = "navigator-arrow";
            toggleLabel.appendChild(arrowSpan);
            combobox.appendChild(toggleLabel);
            const dropdown = document.createElement("div");
            dropdown.className = "navigator-dropdown";
            dropdown.id = `navigator-dropdown-${index}`;
            dropdown.setAttribute('data-path-segment-id', `path-segment-${index}`);
            // get the segments from start to the current one (on UNIX, start with path separator char!)
            const concatenatedPath = (platformType === "Unix" ? '/' : '') + pathSegments.slice(0, index + 1).map(seg => seg.path).join(pathSeparator);
            combobox.setAttribute('data-path', concatenatedPath + (!concatenatedPath.endsWith(pathSeparator) ? pathSeparator : ''));
            combobox.appendChild(dropdown);
            li.appendChild(combobox);
            pathSegmentsContainer.appendChild(li);
        });
    } else
        pathSegmentsContainer.querySelectorAll('li').forEach(li => li.remove()); // no address, clear path segments
}

/**
 * Handle the change event for the comboboxes inside the path-segments.
 * This function is triggered when any of the comboboxes (checkboxes) in the navigation bar is toggled.
 * @param {Event} event - The change event object. 
 */
async function handlePathSegmentComboboxChangeAsync(e) {
    // ensure the event was triggered by an element with the 'navigator-toggle-checkbox' class
    if (e.target.classList.contains('navigator-toggle-checkbox')) {
        const comboboxId = e.target.id;
        const dropdownId = comboboxId.replace('segment-toggle-', 'navigator-dropdown-');
        const dropdown = document.getElementById(dropdownId);
        reattachDropdown();
        if (!e.target.checked)
            return;
        const comboboxElement = e.target.closest('.navigator-combobox'); // get the closest parent (or self) with the specified class
        const pathValue = comboboxElement.getAttribute('data-path');  // retrieve the data-path attribute value
        // clear any existing dropdown elements
        dropdown.innerHTML = "";
        showBusyIndicator();
        setupDropdown(dropdown, e.target);

        // add combobox entry for current directory
        addDirectoryOption(dropdown, pathValue, ".", async () => {
            addressBarInput.value = pathValue;
            await navigateToPathAsync(pathValue, false, false);
        });

        // add combobox entry for parent directory
        addDirectoryOption(dropdown, pathValue, "..", async () => {
            addressBarInput.value = pathValue;
            await navigateUpAsync();
        });

        // fetch directory data for the path
        try {
            await streamFileSystemDirectoriesAsync(pathValue, (parsedItem) => {
                addDirectoryOption(dropdown, pathValue, parsedItem.name, async () => {
                    const newPath = pathValue + (!pathValue.endsWith(pathSeparator) ? pathSeparator : '') + parsedItem.name + pathSeparator;
                    addressBarInput.value = newPath;
                    await navigateToPathAsync(newPath, false, false);
                });
            });
        } catch (error) {
            console.error('Error:', error);
            notificationService.show(error, NotificationType.ERROR);
        } finally {
            hideBusyIndicator();
        }
    }
}

/**
 * Configures and positions the dropdown for directory navigation.
 * @param {HTMLElement} dropdown - The dropdown element to set up.
 * @param {HTMLElement} toggleElement - The element that triggered the dropdown.
 */
function setupDropdown(dropdown, toggleElement) {
    // when drop down opens, need to show address bar overflow, otherwise clipping of drop down occurs
    // store the id of the parent combobox of the dropdown - the dropdown will be reparented
    // because of manadatory "overflow hidden" of scrollable area and clipping isues
    dropdown.setAttribute('data-parent', toggleElement.id);
    dropdown.setAttribute('data-detached', true);
    const position = toggleElement.closest('.navigator-combobox').getBoundingClientRect();
    // detach dropdown and reposition
    document.body.appendChild(dropdown);
    dropdown.style.position = 'absolute';
    dropdown.style.top = `${position.bottom}px`;
    dropdown.style.left = `${position.left}px`;
    dropdown.style.display = 'block';
    // handle addressBar scroll to adjust dropdown position
    addressBar.addEventListener('scroll', adjustDropdownPosition);
}

/**
 * Adds a directory option to the dropdown.
 * @param {HTMLElement} dropdown - The dropdown to add the option to.
 * @param {string} pathValue - The current path.
 * @param {string} name - The name of the directory option.
 * @param {Function} clickHandler - Function to call when the option is clicked.
 */
function addDirectoryOption(dropdown, pathValue, name, clickHandler) {
    const directoryDiv = document.createElement('div');
    directoryDiv.className = "navigator-option";
    directoryDiv.dataset.value = pathValue + (name !== "." && name !== ".." ? name : "");
    directoryDiv.textContent = name;
    directoryDiv.addEventListener('click', clickHandler);
    dropdown.appendChild(directoryDiv);
}

/**
 * Re-attaches the detached visible drop down to the path segment or platform combobox it originally belonged to
 */
function reattachDropdown() {
    // when the combobox closes, re-attach the detached dropdown
    const dropdown = document.querySelector('.navigator-dropdown[data-detached="true"], .enlightenment-dropdown[data-detached="true"]');
    if (dropdown) {
        // reset its absolute positioning and stuff
        dropdown.style.position = '';
        dropdown.style.top = '';
        dropdown.style.left = '';
        dropdown.style.display = '';
        // re-attach it to original parent
        const dataParent = dropdown.getAttribute('data-parent');
        dropdown.removeAttribute('data-detached');
        const originalParent = document.getElementById(dataParent).parentNode;
        if (originalParent)
            originalParent.appendChild(dropdown);
        // remove scroll event listener from the addressBar
        addressBar.removeEventListener('scroll', adjustDropdownPosition);
    }
}

/**
 * Adjusts the position of a detached drop down, such that it is always just under its original 
 * path segment combobox, even when this parent is moved through scrolling
 */
function adjustDropdownPosition() {
    const dropdown = document.querySelector('.navigator-dropdown[data-detached="true"]');
    if (dropdown) {
        const comboboxElement = document.getElementById(dropdown.getAttribute('data-parent')); // get the related combobox
        const position = comboboxElement.parentNode.getBoundingClientRect();
        dropdown.style.top = position.bottom + 'px';
        dropdown.style.left = position.left + 'px';
    }
}

/**
 * Updates the position and height of the dropdown relative to the specified path segment.
 * @param {string} pathSegmentId - The ID of the path segment element to position the dropdown relative to.
 */
function updateDropdownPosition(pathSegmentId, dropdown) {
    const element = document.getElementById(pathSegmentId);
    if (element && dropdown) {
        const rect = getElementOffset(pathSegmentId);
        dropdown.style.top = `${rect.top + rect.height}px`;
        dropdown.style.left = `${rect.left}px`;
    }
}

//+======================================================================================+
//|                                   Tree View                                          |
//+======================================================================================+ 

/**
 * Gets the file system tree view
 * @param {string} initialPath - The initial path to expand in the tree view (optional).
 * @returns {Promise<void>}
 */
async function getFileSystemTreeViewAsync(initialPath) {
    // clear the existing tree view
    fileSystemBrowserTreeview.innerHTML = '';
    // fetch available drives asynchronously
    const drives = await getFileSystemDrivesAsync();
    // iterate through each drive and add it to the tree view as root node
    for (const drive of drives) {
        // create a tree node for the drive
        const driveNode = createFileSystemTreeViewTreeNode(drive);
        // append the drive node to the tree view
        fileSystemBrowserTreeview.appendChild(driveNode);
        // if the initial path starts with this drive's path, expand the tree view to show the initial path
        if (initialPath.toLowerCase().startsWith(drive.path.toLowerCase())) 
            await expandInitialTreeViewPathAsync(driveNode, initialPath);
    }
}

/**
 * Asynchronously fetches the list of available drives from the server.
 * @returns {Promise<Array<Object>|undefined>} A promise that resolves to an array of drive objects, or undefined if the API call fails.
 */
async function getFileSystemDrivesAsync() {
    const getDrivesResponse = await callApiGetAsync(`${clientBasePath}drives/get-drives`);
    if (getDrivesResponse !== undefined)
        return getDrivesResponse.drives;
}

/**
 * Creates a tree node element for the file system tree view.
 * @param {Object} nodeData - Data for the node (path, itemType, name).
 * @returns {HTMLElement} The created tree node element.
 */
function createFileSystemTreeViewTreeNode(nodeData) {
    const treeNodeDiv = document.createElement("div");
    treeNodeDiv.className = "tree-node";
    treeNodeDiv.setAttribute('data-path', nodeData.path);

    const nodeContent = document.createElement("span");
    nodeContent.className = "node-content";
    nodeContent.addEventListener('click', handleNodeClickAsync);

    const expandButton = document.createElement("span");
    expandButton.className = nodeData.itemType !== 'File' ? "expand-button" : "spacer";
    expandButton.addEventListener('click', toggleFileSysemTreeViewNodeExpandAsync);

    const nodeIcon = document.createElement("span");
    nodeIcon.className = "node-icon";
    nodeIcon.textContent = getNodeIcon(nodeData);

    const nodeName = document.createElement("span");
    nodeName.className = "node-name";
    nodeName.textContent = nodeData.name;

    // assemble the node structure
    nodeContent.appendChild(expandButton);
    nodeContent.appendChild(nodeIcon);
    nodeContent.appendChild(nodeName);
    treeNodeDiv.appendChild(nodeContent);

    const childNodes = document.createElement("div");
    childNodes.className = "child-nodes";
    treeNodeDiv.appendChild(childNodes);

    return treeNodeDiv;
}

/**
 * Handles click events on tree nodes.
 * @param {Event} e - The click event object.
 */
async function handleNodeClickAsync(e) {
    // get the node that was clicked
    const treeNode = e.target.closest('.tree-node');
    // retrieve the path stored on the clicked node
    selectedPath = treeNode.getAttribute('data-path');
    if (selectedPath !== null) {
        // ensure path ends with separator
        if (!selectedPath.endsWith(pathSeparator))
            selectedPath += pathSeparator;
        // set the retrieved path as the navigator path, and navigate to it
        addressBarInput.value = selectedPath;
        await navigateToPathAsync(selectedPath, false, false);
    }
}

/**
 * Toggles the expansion of a file system tree view node.
 * @param {Event} e - The click event on the expand button.
 */
async function toggleFileSysemTreeViewNodeExpandAsync(e) {
    e.stopPropagation();
    // get the node that was clicked
    const treeNode = e.target.closest('.tree-node');
    const treeNodeIcon = treeNode.querySelector('.node-icon');
    const nodeData = { itemType: 'Directory' };

    const expandButton = e.target;
    // get the div that hosts child nodes in tthe clicked node
    const childNodes = treeNode.querySelector('.child-nodes');
    const path = treeNode.getAttribute('data-path');
    // get the current expanded state of the clicked node, and toggle it
    const isExpanded = expandButton.classList.contains('expanded');
    nodeData.isExpanded = !isExpanded;
    treeNodeIcon.textContent = getNodeIcon(nodeData);
    // expand or collapse the node
    if (isExpanded) {
        expandButton.classList.remove('expanded');
        childNodes.style.display = 'none';
    } else {
        expandButton.classList.add('expanded');
        childNodes.style.display = 'block';
        // if node is expanded and child nodes are not yet loaded, load them
        if (childNodes.children.length === 0)
            await loadFileSystemTreeViewChildNodesAsync(treeNode, path);
    }
}

/**
 * Loads child nodes for a given parent node in the file system tree view.
 * @param {HTMLElement} parentNode - The parent node element in the tree view.
 * @param {string} path - The file system path to load children from.
 */
async function loadFileSystemTreeViewChildNodesAsync(parentNode, path) {
    // get the div that hosts child nodes
    const childNodes = parentNode.querySelector('.child-nodes');
    // clear existing child nodes
    childNodes.innerHTML = '';
    try {
        showBusyIndicator();
        // stream directory information and create nodes for each
        await streamFileSystemDirectoriesAsync(path, (dir) => {
            dir.itemType = 'Directory';
            const dirNode = createFileSystemTreeViewTreeNode(dir);
            childNodes.appendChild(dirNode);
        });
    } catch (error) {
        console.error('Error:', error);
        notificationService.show(error, NotificationType.ERROR);
    } finally {
        hideBusyIndicator();
    }
}

/**
 * Streams directory information from the server for a given path.
 * @param {string} path - The path to get directories from.
 * @param {Function} callback - Function to call for each directory received.
 */
async function streamFileSystemDirectoriesAsync(path, callback) {
    await streamJsonDataAsync(`${clientBasePath}directories/get-directories?path=${encodeURIComponent(path)}&includeHiddenElements=${showHiddenElements}`, callback, 'Directory');
}

/**
 * Streams file information from the server for a given path.
 * @param {string} path - The path to get files from.
 * @param {Function} callback - Function to call for each file received.
 */
async function streamFileSystemFilesAsync(path, callback) {
    await streamJsonDataAsync(`${clientBasePath}files/get-tree-files?path=${encodeURIComponent(path)}&includeHiddenElements=${showHiddenElements}`, callback, 'File');
}

/**
 * Streams JSON data from a given URL and processes it item by item.
 * This function handles potentially large datasets by processing the data as it arrives,
 * rather than waiting for the entire response to load.
 * 
 * @param {string} url - The URL to fetch JSON data from.
 * @param {Function} callback - Function to call for each processed item.
 * @param {string} itemType - Type of item being processed ('File' or 'Directory').
 */
async function streamJsonDataAsync(url, callback, itemType) {
    // initiate the fetch request from the API
    const response = await fetch(url);
    // get a reader for the response body stream
    const reader = response.body.getReader();
    // create a decoder to convert the incoming byte stream to text
    const decoder = new TextDecoder();   
    let buffer = ''; // buffer to hold incoming data that hasn't been fully processed yet
    // function to process the buffer and extract complete JSON objects
    function processBuffer() {
        let startIndex = 0;
        let endIndex;
        // look for complete JSON objects (ending with '},')
        while ((endIndex = buffer.indexOf('},', startIndex)) !== -1) {
            // extract a single JSON object
            let itemJson = buffer.substring(startIndex, endIndex + 1);
            // remove leading comma if present (from array structure)
            if (itemJson.startsWith(',')) itemJson = itemJson.substring(1);
            // remove surrounding square brackets (array indicators)
            itemJson = itemJson.replace(/^\[|\]$/g, '');
            // parse the JSON string into an object
            var item = JSON.parse(itemJson);
            // manually specify he type of to the parsed object
            item.itemType = itemType;
            // call the provided callback function with the processed item
            callback(item);
            // move the start index to the beginning of the next item
            startIndex = endIndex + 2;
        }
        // keep any incomplete JSON data in the buffer for the next processing cycle
        buffer = buffer.slice(startIndex);
    }
    // main loop to read the stream
    while (true) {
        // read a chunk from the stream
        const { done, value } = await reader.read();
        // if the stream is finished, break the loop
        if (done)
            break;
        // decode the chunk and add it to the buffer
        buffer += decoder.decode(value, { stream: true });
        // process the updated buffer
        processBuffer();
    }
    // process any remaining data in the buffer after the stream is complete
    if (buffer.trim()) {
        let finalJson = buffer.trim();
        if (finalJson.startsWith(',')) finalJson = finalJson.substring(1);
        finalJson = finalJson.replace(/^\[|\]$/g, '');
        if (finalJson !== '') {
            var item = JSON.parse(finalJson);
            item.itemType = itemType;
            callback(item);
        }
    }
}

/**
 * Expands the tree view to a specific target path.
 * @param {HTMLElement} node - The starting node in the tree view.
 * @param {string} targetPath - The full path to expand to.
 */
async function expandInitialTreeViewPathAsync(node, targetPath) {
    const nodePath = node.getAttribute('data-path');
    // exit if the target path doesn't start with the node path
    if (!targetPath.toLowerCase().startsWith(nodePath.toLowerCase()))
        return;
    // expand the current node if it's not already expanded
    const expandButton = node.querySelector('.expand-button');
    if (expandButton && !expandButton.classList.contains('expanded'))
        await toggleFileSysemTreeViewNodeExpandAsync({ target: expandButton, stopPropagation: () => { } });
    // split the remaining path into segments
    const pathSegments = targetPath.slice(nodePath.length).split('\\').filter(Boolean);
    let currentNode = node;
    // iterate through each path segment
    for (let i = 0; i < pathSegments.length; i++) {
        const segment = pathSegments[i];
        const childNodes = currentNode.querySelector('.child-nodes');
        let found = false;
        // search for the child node matching the current path segment
        for (const childNode of childNodes.children) {
            if (childNode.querySelector('.node-name').textContent.toLowerCase() === segment.toLowerCase()) {
                currentNode = childNode;
                found = true;
                if (i === pathSegments.length - 1) {
                    // if this is the last segment (target node)
                    childNode.scrollIntoView(); // bring it into view, if its not already
                    // mark it as selected
                    childNode.querySelector('.node-content').classList.add('selected');
                    // toggle this last node, because all its children will be loaded
                    const expandChildButton = childNode.querySelector('.expand-button');
                    if (expandChildButton)
                        await toggleFileSysemTreeViewNodeExpandAsync({ target: expandChildButton, stopPropagation: () => { } });
                    // load the node's children
                    await loadFileSystemTreeViewChildNodesAsync(childNode, childNode.getAttribute('data-path'));
                }
                else {
                    // if this is an intermediate node, expand it if necessary
                    const childExpandButton = childNode.querySelector('.expand-button');
                    if (childExpandButton && !childExpandButton.classList.contains('expanded'))
                        await toggleFileSysemTreeViewNodeExpandAsync({ target: childExpandButton, stopPropagation: () => { } });
                }
                break;
            }
        }
        // if the current segment wasn't found, stop expanding
        if (!found)
            break;
    }
}

//+======================================================================================+
//|                                  File System                                         |
//+======================================================================================+ 

/**
 * Fetches and displays file system items for a given path.
 * @param {string} path - The path to fetch items from.
 */
async function getFileSystemItemsAsync(path) {
    // clear the current contents of the file system explorer
    fileSystemExplorer.innerHTML = '';

    try {
        showBusyIndicator();
        // stream file system items and create explorer items for each
        await streamFileSystemItemsAsync(path, (item) => {
            const newItem = createFileSystemExplorerItem(item);
            fileSystemExplorer.appendChild(newItem);
        });
    } catch (error) {
        console.error('Error:', error);
        notificationService.show(error, NotificationType.ERROR);
    } finally {
        hideBusyIndicator();
    }
}

/**
 * Streams both directories and files from a specified path.
 * @param {string} path - The file system path to stream items from.
 * @param {Function} callback - Function to call for each item (directory or file) received.
 */
async function streamFileSystemItemsAsync(path, callback) {
    await streamFileSystemDirectoriesAsync(path, callback);
    await streamFileSystemFilesAsync(path, callback);
}

/**
 * Creates a DOM element representing a file system item (file or directory).
 * @param {Object} item - The file system item data.
 * @returns {HTMLElement} The created DOM element for the file system item.
 */
function createFileSystemExplorerItem(item) {
    const itemDiv = document.createElement('div');
    const itemType = item.itemType;
    // set up the main item div with appropriate classes and data attributes
    itemDiv.className = `e ${viewMode}`;
    itemDiv.setAttribute('data-path', item.path);
    itemDiv.setAttribute('data-type', itemType);
    // create and set up the icon div
    const iconDiv = document.createElement('div');
    iconDiv.className = 'icon';
    iconDiv.innerHTML = `<img src='/images/icons/${CURRENT_ICON_THEME}/${getIconPathForFile(item.path, itemType)}'>`;
    // create and set up the text div
    const textDiv = document.createElement('div');
    textDiv.className = 'text';
    // use different class for directory ('d') and file ('f')
    textDiv.innerHTML = `<span class='${itemType === 'Directory' ? 'd' : 'f'} t'>${item.name}</span>`;
    // assemble the item structure
    itemDiv.appendChild(iconDiv);
    itemDiv.appendChild(textDiv);
    // add event listeners for mouse interactions
    itemDiv.onmouseup = handleFileSystemEntityMouseUpAsync;
    itemDiv.onmousedown = handleFileSystemEntityMouseDown;
    return itemDiv;
}

/**
 * Handles mouse up events on file system entities in the explorer view.
 * Manages selection, double-click actions, and navigation.
 * @param {MouseEvent} e - The mouse event.
 */
async function handleFileSystemEntityMouseUpAsync(e) {
    e.preventDefault();
    if (!isSelecting)
        e.stopPropagation();
    const allEClassElements = Array.from(fileSystemExplorer.querySelectorAll('.e'));
    const currentIndex = allEClassElements.indexOf(e.currentTarget);

    // if a previous click timestamp exists, check the time difference
    const previousTime = parseInt(e.currentTarget.dataset.selectionStartTime || 0);
    const currentTime = new Date().getTime();
    const timeDifference = currentTime - previousTime;

    // TODO: needs to account for when the element is already selected, and a double click occurs - on first click it would check the difference and go to rename -
    // maybe just wait half second before performing action

    // if time difference is less than 300ms, treat it as a double-click
    if (timeDifference < 300) {
        if (e.currentTarget.dataset.type === 'Directory') { // directories
            addressBarInput.value = e.currentTarget.dataset.path;
            await navigateToPathAsync(addressBarInput.value, false, false); // TODO: add to undo 
            return;
        } else { // files

        }
        // reset the timestamp used to track double clicks
        e.currentTarget.dataset.selectionStartTime = 0;
        return;
    }
    else
        e.currentTarget.dataset.selectionStartTime = currentTime; // element was single clicked, update the selection start time to now 
    if (e.ctrlKey || isSelectionMode) { // when CTRL key is pressed
        // toggle selection for the current clicked item
        e.currentTarget.classList.toggle('selected');
        // set the selection start if it's not already set
        if (fileSystemExplorer.dataset.selectionStart === undefined)
            fileSystemExplorer.dataset.selectionStart = currentIndex;
    }
    else if ((e.shiftKey && fileSystemExplorer.dataset.selectionStart !== undefined) || isSelectionMode) { // when SHIFT key is pressed
        const start = parseInt(fileSystemExplorer.dataset.selectionStart);
        // remove all selections
        allEClassElements.forEach(elem => {
            elem.classList.remove('selected');
        });
        // define the range to select items from and to
        const [from, to] = start < currentIndex ? [start, currentIndex] : [currentIndex, start];
        // add the 'selected' class to items in the range
        for (let i = from; i <= to; i++)
            allEClassElements[i].classList.add('selected');
    }
    else { // when neither CTRL nor SHIFT keys are pressed
        if (!isSelectionMode) {
            // get all elements that are selected
            const selectedElements = allEClassElements.filter(element => element.classList.contains('selected'));
            // add 'selected' class to the current clicked item
            e.currentTarget.classList.add('selected');
            // if there are no selected elements, or if there are more than one, deselect all of them, and select just the clicked one
            if ((selectedElements.length === 1 && e.currentTarget !== selectedElements[0]) || selectedElements.length !== 1) {
                // deselect all items
                selectedElements.forEach(elem => {
                    if (elem !== e.currentTarget) {
                        elem.classList.remove('selected');
                        // remove selectionStartTime from all items
                        delete elem.dataset.selectionStartTime;
                    }
                });
                // set the current item as the selection start
                fileSystemExplorer.dataset.selectionStart = currentIndex;
                // store the current timestamp for future double-click checks
                e.currentTarget.dataset.selectionStartTime = currentTime;
            }
        }
        else // if its selection mode, just toggle selection of item, when clicked
            e.currentTarget.classList.toggle('selected');
    }
    const selectedElements = document.querySelectorAll('.e.selected');
    btnFileSystemBrowserConfirm.disabled = selectedElements.length === 0; // disable Confirm button if no item is selected
    if (selectedElements.length > 0) {
        var selectedPaths = '"';
        selectedElements.forEach((element) => {
            selectedPaths += element.dataset.path + '","';
        });
        selectedFileSystemElementsInput.value = selectedPaths.substring(0, selectedPaths.length - 2);
    }
    else
        selectedFileSystemElementsInput.value = '';
}

/**
 * Handler for the mouse down event of file system browser dialog files and directories elements.
 * @param {any} event
 */
function handleFileSystemEntityMouseDown(e) {
    // stop the event from bubbling up to the explorer - we dont want to start selection rectangles when clicking files or directories
    e.stopPropagation();
}

//+======================================================================================+
//|                                   View Mode                                          |
//+======================================================================================+ 

/**
 * Switches to a different view mode.
 * @param {string} mode - The view mode to switch to ('list', 'details', or 'icons').
 * @param {string|null} iconSize - The icon size for 'icons' mode ('small', 'medium', 'large', 'extra-large'). Use null for non-icon modes.
 * @param {boolean} isManualSet - Indicates whether the mode change is triggered manually (by clicking a mode change icon) or automatically.
 */
function switchViewMode(mode, iconSize, isManualSet) {
    hasScrolledAfterModeChange = false; // reset the flag indicating a mode change
    setViewMode(mode, iconSize);
    // check if content has been scrolled after the mode change; since scroll is the one triggering thumbnails retrieval,
    // and there is no initial scroll when mode is changed, we need to trigger the retrieval manually
    setTimeout(() => {
        if (!hasScrolledAfterModeChange && showThumbnails && isManualSet) getFileSystemVisibleItems();
    }, 50); // wait 50ms to ensure all other events have processed
}

/**
 * Sets the explorer view mode.
 * @param {string} mode - View mode ('list', 'details', or 'icons').
 * @param {string} [iconSize] - Icon size for 'icons' mode ('small', 'medium', 'large', 'extra-large').
 */
function setViewMode(mode, iconSize) {
    if (!fileSystemExplorer)
        return;
    const isIconMode = mode === 'icons';
    const viewClass = isIconMode ? 'details' : mode;
    const iconClass = isIconMode ? `${iconSize}-icons` : `${mode}-icons`;
    viewMode = iconClass;
    fileSystemExplorer.className = `${viewClass}`;
    fileSystemExplorer.style.flexDirection = mode === 'list' ? 'column' : isIconMode ? 'row' : 'column';

    fileSystemExplorerContainer.classList.toggle('scroll-horizontal', mode === 'list');
    fileSystemExplorerContainer.classList.toggle('scroll-vertical', mode !== 'list');

    fileSystemDetailsHeader.style.height = mode === 'details' ? '20px' : '0px';
    fileSystemDetailsHeader.style.position = mode === 'details' ? 'sticky' : 'static';
    fileSystemDetailsHeader.style.top = mode === 'details' ? '-1px' : '';

    setEnabledViewModeIcon(isIconMode ? `${iconSize}-icons` : mode);
    mode === 'list' ? addHorizontalScrolling(fileSystemExplorer) : removeHorizontalScrolling(fileSystemExplorer);

    fileSystemExplorer.querySelectorAll('.e').forEach(el => {
        el.className = `e ${iconClass}`;
    });
    mode === 'details' ? showExtraDetails() : hideExtraDetails();
}

/**
 * Hides additional item details such as date modified, type, and size.
 */
function hideExtraDetails() {
    fileSystemExplorer.querySelectorAll('.date-modified, .type, .size').forEach(el => {
        el.style.display = 'none';
    });
}

/**
 * Shows additional item details such as date modified, type, and size.
 */
function showExtraDetails() {
    fileSystemExplorer.querySelectorAll('.date-modified, .type, .size').forEach(el => {
        el.style.display = 'flex';
    });
}

/**
 * Sets the enabled view mode icon.
 * @param {string} viewType - The type of view to enable ('list', 'details', 'small-icons', 'medium-icons', 'large-icons', 'extra-large-icons').
 */
function setEnabledViewModeIcon(viewType) {
    const allViews = [
        svgListView,
        svgDetailsView,
        svgSmallIconsView,
        svgMediumIconsView,
        svgLargeIconsView,
        svgExtraLargeIconsView
    ];

    allViews.forEach((view, index) => {
        view.forEach(element => {
            if (index === ['list', 'details', 'small-icons', 'medium-icons', 'large-icons', 'extra-large-icons'].indexOf(viewType))
                element.classList.add('enabled');
            else
                element.classList.remove('enabled');
        });
    });
}

//+======================================================================================+
//|                                   Selection                                          |
//+======================================================================================+ 

/**
 * Handler for the file system browser dialog selection start.
 * @param {MouseEvent} e - The mousedown event object.
 */
function startSelection(e) {
    const explorerScrollbars = hasScrollbars(fileSystemExplorer);
    const containerExplorerScrollbars = hasScrollbars(fileSystemExplorerContainer);
    // check if clicked on the horizontal or vertical scrollbar
    if (explorerScrollbars.horizontal && e.clientY > fileSystemExplorer.getBoundingClientRect().top + fileSystemExplorer.offsetHeight - scrollbarHeight)
        return;
    if (containerExplorerScrollbars.vertical && e.clientX > fileSystemExplorerContainer.getBoundingClientRect().left + fileSystemExplorerContainer.offsetWidth - scrollbarWidth)
        return;
    // store information about selection start
    isSelecting = true;
    document.body.style.overflowY = 'hidden'; // needed in order to not put a vertical scrollbar on the document, while in vertical layout selection
    document.documentElement.style.overflowY = 'hidden';
    selectionStartPosition.x = e.pageX + fileSystemExplorer.scrollLeft;
    selectionStartPosition.y = e.pageY + fileSystemExplorerContainer.scrollTop; // not a mistake - in vertical mode, this is the overflow container!
    // place the selection rectangle at the clicked location (relative to the whole page, and taking into account scroll offset), with 0 size
    visibleSelectionRectangle.style.left = e.pageX + 'px';
    visibleSelectionRectangle.style.top = e.pageY + 'px';
    visibleSelectionRectangle.style.width = '0px';
    visibleSelectionRectangle.style.height = '0px';
    visibleSelectionRectangle.style.visibility = 'visible';

    selectionRectangle.style.left = e.pageX + 'px';
    selectionRectangle.style.top = e.pageY + 'px';
    selectionRectangle.style.width = '0px';
    selectionRectangle.style.height = '0px';
}

/**
 * Handler for the file system browser dialog selection update.
 * @param {MouseEvent} e - The mousemove event object.
 */
function updateSelection(e) {
    if (!isSelecting)
        return;
    currentMousePosition.x = e.pageX;
    currentMousePosition.y = e.pageY;
    handleSelection();
}

/**
 * Handler for the file system browser dialog selection end.
 */
function endSelection() {
    // selection ended, hide the selection rectangle
    isSelecting = false;
    document.body.style.removeProperty('overflow-y');
    document.documentElement.style.removeProperty('overflow-y');

    visibleSelectionRectangle.style.visibility = 'hidden';
    selectionRectangle.style.visibility = 'hidden';

    selectionRectangle.style.left = '0px';
    selectionRectangle.style.top = '0px';
    selectionRectangle.style.width = '0px';
    selectionRectangle.style.height = '0px';
    visibleSelectionRectangle.style.left = '0px';
    visibleSelectionRectangle.style.top = '0px';
    visibleSelectionRectangle.style.width = '0px';
    visibleSelectionRectangle.style.height = '0px';
    // go through each .e item that was inside the selection rectangle, and mark it as selected
    fileSystemExplorer.querySelectorAll('.e.selection-hover').forEach(item => {
        item.classList.add('selected');
        item.classList.remove('selection-hover');
    });
    const selectedElements = document.querySelectorAll('.e.selected');
    btnFileSystemBrowserConfirm.disabled = selectedElements.length === 0; // disable Confirm button if no item is selected
    if (selectedElements.length > 0) {
        var selectedPaths = '"';
        selectedElements.forEach((element) => {
            selectedPaths += element.dataset.path + '","';
        });
        selectedFileSystemElementsInput.value = selectedPaths.substring(0, selectedPaths.length - 2);
    }
    else
        selectedFileSystemElementsInput.value = '';
}

/**
 * Function for handling the file system browser dialog selection.
 */
function handleSelection() {
    if (!isSelecting) // only relevant when we have a selection initialized
        return;
    // calculate dimensions of the selection rectangle
    const currentX = currentMousePosition.x;
    const currentY = currentMousePosition.y;
    const width = currentX - (selectionStartPosition.x - fileSystemExplorer.scrollLeft);
    const height = currentY - (selectionStartPosition.y - fileSystemExplorerContainer.scrollTop);

    // update selection rectangle dimensions
    visibleSelectionRectangle.style.width = Math.abs(width) + 'px';
    visibleSelectionRectangle.style.height = Math.abs(height) + 'px';
    selectionRectangle.style.width = Math.abs(width) + 'px';
    selectionRectangle.style.height = Math.abs(height) + 'px';

    // adjust position based on selection direction
    if (width < 0) {
        selectionRectangle.style.left = currentX + 'px';
        visibleSelectionRectangle.style.left = currentX + 'px';
    } else {
        selectionRectangle.style.left = (selectionStartPosition.x - fileSystemExplorer.scrollLeft) + 'px';
        visibleSelectionRectangle.style.left = (selectionStartPosition.x - fileSystemExplorer.scrollLeft) + 'px';
    }
    if (height < 0) {
        selectionRectangle.style.top = currentY + 'px';
        visibleSelectionRectangle.style.top = currentY + 'px';
    } else {
        selectionRectangle.style.top = (selectionStartPosition.y - fileSystemExplorerContainer.scrollTop) + 'px';
        visibleSelectionRectangle.style.top = (selectionStartPosition.y - fileSystemExplorerContainer.scrollTop) + 'px';
    }

    // do not let the selection rectangle go outside of explorer container, clip it if needed
    const containerRect = fileSystemExplorerContainer.getBoundingClientRect();
    const left = parseFloat(visibleSelectionRectangle.style.left);
    const top = parseFloat(visibleSelectionRectangle.style.top) + 1;
    const right = left + parseFloat(visibleSelectionRectangle.style.width);
    const bottom = top + parseFloat(visibleSelectionRectangle.style.height);

    if (left < containerRect.left) {
        const difference = containerRect.left - left;
        visibleSelectionRectangle.style.width = (parseFloat(visibleSelectionRectangle.style.width) - difference) + 'px';
        visibleSelectionRectangle.style.left = containerRect.left + 'px';
    }
    if (right > containerRect.right)
        visibleSelectionRectangle.style.width = (containerRect.right - left) + 'px';

    if (top < containerRect.top) {
        const difference = containerRect.top - top;
        visibleSelectionRectangle.style.height = (parseFloat(visibleSelectionRectangle.style.height) - difference) + 'px';
        visibleSelectionRectangle.style.top = containerRect.top + 'px';
    }
    if (bottom > containerRect.bottom)
        visibleSelectionRectangle.style.height = (containerRect.bottom - top) + 'px';

    // change color of items that fall within selection rectangle            
    const selectionRect = selectionRectangle.getBoundingClientRect();
    // go through each .e item which has not had its thumbnail retrieved yet
    fileSystemExplorer.querySelectorAll('.e').forEach(item => {
        const itemRect = item.getBoundingClientRect();
        // check if the item is fully or partially visible horizontally and vertically
        const isFullyVisibleHorizontally = itemRect.left >= selectionRect.left && itemRect.right <= selectionRect.right;
        const isFullyVisibleVertically = itemRect.top >= selectionRect.top && itemRect.bottom <= selectionRect.bottom;
        const isPartiallyVisibleHorizontally = itemRect.left < selectionRect.right && itemRect.right > selectionRect.left;
        const isPartiallyVisibleVertically = itemRect.top < selectionRect.bottom && itemRect.bottom > selectionRect.top;
        // if the item is fully or partially visible in both directions, add it to our list
        if ((isFullyVisibleHorizontally || isPartiallyVisibleHorizontally) && (isFullyVisibleVertically || isPartiallyVisibleVertically))
            item.classList.add('selection-hover');
        else
            item.classList.remove('selection-hover');
    });
}

/**
 * Enables or disables the display of the file system tree view.
 * @param {MouseEvent} e - The mouseup event object.
 */
function toggleFileSystemSelectionMode(e) {
    isSelectionMode = !isSelectionMode;
    e.preventDefault();
    e.stopPropagation();

    svgSelectionMode.forEach(element => {
        isSelectionMode ? element.classList.add('enabled') : element.classList.remove('enabled');
    });
}

/**
 * Handler for the mouse up event of the file system browser dialog explorer
 * @param {MouseEvent} e - The mouseup event object.
 */
function handleFileSystemExplorerMouseUp(e) {
    const explorerScrollbars = hasScrollbars(fileSystemExplorer);
    // check if clicked on the horizontal scrollbar
    if (explorerScrollbars.horizontal && e.clientY > fileSystemExplorer.getBoundingClientRect().top + fileSystemExplorer.offsetHeight - scrollbarHeight)
        return;
    if (!e.ctrlKey && !e.shiftKey && !isSelectionMode) {
        // deselect all file system elements when their explorer is clicked
        fileSystemExplorer.querySelectorAll('.e.selected').forEach(item => {
            item.classList.remove('selected');
        });
        // do not store information about what item was clicked first, in context of Shift-selection
        fileSystemExplorer.removeAttribute('data-selection-start');
        btnFileSystemBrowserConfirm.disabled = document.querySelectorAll('.e.selected').length === 0; // disable Confirm button if no item is selected
    }
}

/**
 * Handles the scroll event for the explorer or explorerContainer.
 * Clears any previous timeout and creates a new one to fetch visible items.
 */
function handleFileSystemBrowserScrollEvent() {
    // clear any previously set timeouts to debounce the getFileSystemVisibleItems call
    if (scrollTimeout) clearTimeout(scrollTimeout);
    // if there's an ongoing operation, abort it
    if (abortController) {
        abortController.abort();
        abortController = null;
    }
    // set a timeout to call getFileSystemVisibleItems once the user stops scrolling
    scrollTimeout = setTimeout(() => {
        if (showThumbnails)
            getFileSystemVisibleItems();
    }, scrollThumbnailRetrievalTimeout);
    hasScrolledAfterModeChange = true;
}

/**
 * Handles the window's resize event.
 * Clears any previous timeout and creates a new one to fetch file system browser visible items.
 */
function handleFileSystemBrowserResizeEvent() {
    // clear any previously set timeouts to debounce the getFileSystemVisibleItems call during resize
    if (resizeTimeout) clearTimeout(resizeTimeout);
    // if there's an ongoing operation, abort it
    if (abortController) {
        abortController.abort();
        abortController = null;
    }
    // set a timeout to call getFileSystemVisibleItems once the user stops resizing
    resizeTimeout = setTimeout(() => {
        if (showThumbnails)
            getFileSystemVisibleItems();
    }, scrollThumbnailRetrievalTimeout);
}

//+======================================================================================+
//|                                   Thumbnails                                         |
//+======================================================================================+ 

/**
 * Identifies items within the explorerContainer that are either fully or partially visible.
 * Items with thumbnails already retrieved are excluded.
 * Populates a list of such visible items and passes them for further processing.
 */
function getFileSystemVisibleItems() {
    const containerRect = fileSystemExplorerContainer.getBoundingClientRect();
    const visibleItems = [];
    // go through each .e item which has not had its thumbnail retrieved yet
    const fileElements = Array.from(fileSystemExplorer.querySelectorAll('.e:not([data-thumbnail-retrieved])[data-type="File"]'));
    // if the option for checking file header bytes is disabled, filter images by extension
    const itemsToProcess = inspectFileForThumbnails ? fileElements : fileElements.filter(item => isImageFile(item.getAttribute('data-path')));
    itemsToProcess.forEach(item => {
        const itemPath = item.getAttribute('data-path');
        const itemRect = item.getBoundingClientRect();
        // check if the item is fully or partially visible horizontally and vertically
        const isFullyVisibleHorizontally = itemRect.left >= containerRect.left && itemRect.right <= containerRect.right;
        const isFullyVisibleVertically = itemRect.top >= containerRect.top && itemRect.bottom <= containerRect.bottom;
        const isPartiallyVisibleHorizontally = itemRect.left < containerRect.right && itemRect.right > containerRect.left;
        const isPartiallyVisibleVertically = itemRect.top < containerRect.bottom && itemRect.bottom > containerRect.top;
        // if the item is fully or partially visible in both directions, add it to our list
        if ((isFullyVisibleHorizontally || isPartiallyVisibleHorizontally) &&
            (isFullyVisibleVertically || isPartiallyVisibleVertically)) {
            const imgElement = item.querySelector('img');
            visibleItems.push({
                path: itemPath,
                img: imgElement
            });
        }
    });
    // process the list of visible items
    processVisibleItems(visibleItems);
}

/**
 * Processes a given item by retrieving its thumbnail from an API and updating the item's thumbnail image.
 * If there are more items in the visibleItems list, it continues to process the next item.
 * If an error occurs during the retrieval process (including an aborted fetch operation), it is handled gracefully.
 * @param {Object} item - The item containing its path and corresponding image element.
 * @param {Array} visibleItems - List of remaining visible items to process.
 * @throws {AbortError} - Indicates the fetch operation was aborted.
 * @throws {Error} - Catches and logs other errors that might occur during the thumbnail retrieval.
 */
async function processItem(item, visibleItems) {
    try {
        const result = await getThumbnailApiCall(item.path, imagePreviewsQuality);
        // update the item's thumbnail using the retrieved data
        if (typeof result.base64Data !== "undefined")
            item.img.src = `data:${result.mimeType};base64,${result.base64Data}`;
        item.img.closest('.e').setAttribute('data-thumbnail-retrieved', 'true');
        // continue with next item if there are more in the queue
        if (visibleItems.length > 0) {
            const nextItem = visibleItems.shift();
            processItem(nextItem, visibleItems);
        }
    } catch (error) {
        if (error.name === 'AbortError') {
            // console.log('Fetch operation was aborted.');
        } else {
            // on error, revert the icons to the default ones
            let itemType = item.img.closest('.e').getAttribute('data-type');
            if (itemType === 'directory')
                item.img.src = clientBasePath + "images/icons/" + CURRENT_ICON_THEME + "/directory.svg";
            else if (itemType === 'file')
                item.img.src = clientBasePath + "images/icons/" + CURRENT_ICON_THEME + "/file.svg";
            console.error('Error processing item:', error);
        }
    }
}

/**
 * Makes an API call to retrieve a thumbnail for the given item.
 * @param {string} path - The path of the item for which the thumbnail needs to be fetched.
 * @returns {Promise<object>} - Returns a Promise resolving to an object containing base64Data and mimeType.
 * @throws {Error} - Throws an error if the API call fails.
 */
async function getThumbnailApiCall(path, quality) {
    // fetch the thumbnail for the given item from the server
    const response = await fetch(`${serverBasePath}thumbnails/get-thumbnail?path=` + encodeURIComponent(path) + '&quality=' + quality, {
        signal: abortController.signal // use the abort signal 
    });
    if (!response.ok)
        throw new Error('Failed to fetch the thumbnail');
    // convert the response to a JSON object
    const blob = await response.blob();
    const base64Data = await blobToBase64(blob);
    const mimeType = response.headers.get('Content-Type');
    return {
        base64Data: base64Data,
        mimeType: mimeType
    };
}

/**
 * Initiates the processing of visible items. Processing is batched based on thumbnailsRetrievalBatchSize. * 
 * @param {Array} visibleItemsList - List of visible items to be processed.
 */
async function processVisibleItems(visibleItemsList) {
    const visibleItems = [...visibleItemsList];
    abortController = new AbortController(); // create a new AbortController
    // process items in batches determined by thumbnailsRetrievalBatchSize
    for (let i = 0; i < Math.min(thumbnailsRetrievalBatchSize, visibleItems.length); i++) {
        const item = visibleItems.shift();
        processItem(item, visibleItems);
    }
}


/**
 * Returns the path to an SVG icon based on the file extension.
 * @param {string} filename - The full name of the file including its extension.
 * @returns {string} - The path to the appropriate SVG icon or a default one if not found.
 */
function getIconPathForFile(filename, type) {
    // extract file extension from filename
    const fileExtension = filename.split('.').pop().toLowerCase();
    // return the appropriate SVG image path or a default one if not found
    return FILE_ICONS[fileExtension] || (type === "File" ? "file.svg" : "directory.svg");
}

//+======================================================================================+
//|                                  Miscellaneous                                       |
//+======================================================================================+ 

/**
 * Enables or disables the display of the file system tree view.
 * @param {boolean} isManualSet - Indicates whether the toggle is triggered manually (by clicking on the icon) or automatically.
 */
async function toggleFileSystemTreeViewAsync(isManualSet) {
    if (isManualSet)
        showFileSystemTreeView = !showFileSystemTreeView;

    showFileSystemTreeView ? fileSystemTreeViewContainer.classList.remove('collapsed') : fileSystemTreeViewContainer.classList.add('collapsed');
    svgTreeView.forEach(element => {
        showFileSystemTreeView ? element.classList.add('enabled') : element.classList.remove('enabled');
    });
    showFileSystemTreeView ? fileSystemTreeViewContainer.classList.remove('collapsed') : fileSystemTreeViewContainer.classList.add('collapsed');
    // if file system treeview needs to be displayed, get its file system tree structure from the API
    if (showFileSystemTreeView && isManualSet && addressBarInput.value !== null) {
        if (!addressBarInput.value.endsWith(pathSeparator))
            addressBarInput.value += pathSeparator;
        await navigateToPathAsync(addressBarInput.value, false, false);
    }
}

/**
 * Enables or disables the display of hidden file system elements.
 * @param {boolean} isManualSet - Indicates whether the toggle is triggered manually (by clicking on the icon) or automatically.
 */
async function toggleShowFileSystemHiddenElementsAsync(isManualSet) {
    if (isManualSet)
        showHiddenElements = !showHiddenElements;

    svgHiddenElements.forEach(element => {
        showHiddenElements ? element.classList.add('enabled') : element.classList.remove('enabled');
    });
    if (isManualSet && addressBarInput.value !== null) {
        if (!addressBarInput.value.endsWith(pathSeparator))
            addressBarInput.value += pathSeparator;
        await navigateToPathAsync(addressBarInput.value, false, false);
    }
}

/**
 * Closes all comboboxes except for the one passed as an argument.
 * @param {HTMLElement} exceptionCheckbox - The checkbox of the combobox that should remain open.
 */
function closeAllComboboxesExcept(exceptionCheckbox) {
    const checkedCheckboxes = document.querySelectorAll('.enlightenment-toggle-checkbox:checked, .navigator-toggle-checkbox:checked');
    checkedCheckboxes.forEach(function (checkbox) {
        if (!exceptionCheckbox || checkbox.id !== exceptionCheckbox.id)
            checkbox.checked = false;
    });
}

/**
 * Gets the icon of the node, based on it type.
 * @param {any} node The node for which to get the icon.
 * @returns The icon on the node.
 */
function getNodeIcon(node) {
    switch (node.itemType) {
        case "Root":
            return "💿";
        case "Directory":
            return node.isExpanded ? "📂" : "📁";
        case "File":
            return "📄";
        default:
            return "❓";
    }
}

//+======================================================================================+
//|                                Event Handlers                                        |
//+======================================================================================+ 

// event handlers for changing file system view modes
btnListView.addEventListener('click', () => switchViewMode('list', null, true));
btnDetailsView.addEventListener('click', () => switchViewMode('details', null, true));
btnSmallIconsView.addEventListener('click', () => switchViewMode('icons', 'small', true));
btnMediumIconsView.addEventListener('click', () => switchViewMode('icons', 'medium', true));
btnLargeIconsView.addEventListener('click', () => switchViewMode('icons', 'large', true));
btnExtraLargeIconsView.addEventListener('click', () => switchViewMode('icons', 'extra-large', true));

btnNavigatorToggleTreeView.addEventListener('click', async () => await toggleFileSystemTreeViewAsync(true));
btnNavigatorToggleSelectionMode.addEventListener('click', toggleFileSystemSelectionMode);
btnNavigatorHiddenElements.addEventListener('click', async () => await toggleShowFileSystemHiddenElementsAsync(true));
btnNavigatorNavigateUp.addEventListener('click', navigateUpAsync);

fileSystemExplorer.addEventListener('mouseup', handleFileSystemExplorerMouseUp);
fileSystemExplorer.addEventListener('scroll', handleFileSystemBrowserScrollEvent);

fileSystemExplorerContainer.addEventListener('mousedown', startSelection);
fileSystemExplorerContainer.addEventListener('mousemove', updateSelection);
fileSystemExplorerContainer.addEventListener('mouseup', endSelection);
fileSystemExplorerContainer.addEventListener('wheel', handleSelection, { passive: true });
fileSystemExplorerContainer.addEventListener('scroll', handleSelection);

fileSystemExplorerContainer.addEventListener('scroll', handleSelection);

/**
 * Event handler for the address bar input navigate button Click event.
 */
btnFileSystemBrowserSubmitPath.addEventListener('click', async function () {
    await navigateToPathAsync(addressBarInput.value, false, false);
    addressBarInput.style.display = 'none';
    addressBar.style.display = 'block';
});

/**
 * Event handler for the address bar input edit button Click event.
 */
btnFileSystemBrowserEditPath.addEventListener('click', function () {
    addressBarInput.style.display = 'block';
    addressBarInput.focus();
    addressBar.style.display = 'none';
});

/**
 * Event handler for the navigator back button Click event.
 */
btnNavigatorNavigateBack.addEventListener('click', async function () {
    if (addressBarInput.value !== null) 
        await navigateToPathAsync(addressBarInput.value, false, true);
});

/**
 * Event handler for the navigator address bar input KeyDown event.
 */
addressBarInput.addEventListener('keydown', async function (e) {
    if (e.key === 'Escape') {
        e.preventDefault();
        addressBarInput.style.display = 'none';
        addressBar.style.display = 'block'; 
    } else if (e.key === 'Enter') 
        await navigateToPathAsync(addressBarInput.value, false, false);
});

/**
 * Event handler for the addressbar Click event.
 */
addressBar.addEventListener('click', function (e) {
    // if the clicked element is the ul itself or one of its direct children (but not deeper nested children)
    if (e.target === e.currentTarget || e.target.parentElement === e.currentTarget) {
        this.style.display = 'none'; // hide #addressBar
        addressBarInput.style.display = 'block'; // show #addressBarInput
        addressBarInput.focus(); // focus on the input
    }
});

/**
 * Detects changes on any combobox checkbox. If a combobox is opened, this will ensure all other comboboxes are closed.
 */
document.addEventListener('change', function (e) {
    if (e.target.matches('.enlightenment-toggle-checkbox, .navigator-toggle-checkbox')) 
        if (e.target.checked) 
            closeAllComboboxesExcept(e.target);
});

/**
 * Event handler for when an option in the combobox is clicked.
 * This updates the displayed value of the combobox to the clicked option and closes the dropdown.
 */
document.addEventListener('click', function (e) {
    if (e.target.matches('.enlightenment-option')) {
        var text = e.target.textContent;
        var combobox = e.target.closest('.enlightenment-combobox');

        var selectedText = combobox.querySelector('.enlightenment-selected-text');
        if (selectedText) 
            selectedText.textContent = text;

        var toggleCheckbox = combobox.querySelector('.enlightenment-toggle-checkbox');
        if (toggleCheckbox) 
            toggleCheckbox.checked = false;
    }
});

/**
 * Global click event for the entire document. Closes all combobox dropdowns if the clicked target is outside any combobox.
 */
document.addEventListener('click', function (e) {
    // enlightenment combobox logic
    if (!e.target.closest('.enlightenment-combobox')) {
        const checkbox = document.querySelector('.enlightenment-toggle-checkbox:checked');
        if (checkbox) 
            checkbox.checked = false;
    }
    // navigator combobox logic
    if (!e.target.closest('.navigator-combobox') && addressBar) {
        const checkbox = document.querySelector('.navigator-toggle-checkbox:checked');
        if (checkbox) 
            checkbox.checked = false;
        // when drop down closes, hide address bar overflow
        addressBar.style.overflowX = 'auto';
        addressBar.style.overflowY = 'hidden';
        reattachDropdown();
    }
});

/**
 * Event handler for navigation combobox drop down item clicks.
 */
document.addEventListener("click", function (e) {
    if (e.target.matches('.navigator-option')) {
        // the address bar drop downs may be destroyed when an option is clicked - they are regenerated each time anyway
        addressBar.removeEventListener('scroll', adjustDropdownPosition);

        var parentElement = e.target.parentElement;
        if (parentElement) 
            parentElement.remove();
    }
});

/**
 * Prevents the global document click event from propagating when clicking on a combobox. 
 * This ensures the dropdown doesn't close unintentionally.
 */
document.addEventListener('click', function (e) {
    if (e.target.closest('.enlightenment-combobox, .navigator-combobox')) 
        e.stopPropagation();
});

/**
 * Handles the window resize event.
 */
window.addEventListener('resize', function () {
    handleFileSystemBrowserResizeEvent();
    // update all dropdowns' positions on resize
    const dropdown = document.querySelector('.navigator-dropdown[data-detached="true"]');
    if (dropdown) {
        const pathSegmentId = dropdown.getAttribute('data-path-segment-id');
        if (pathSegmentId)
            updateDropdownPosition(pathSegmentId, dropdown);
    }
});
