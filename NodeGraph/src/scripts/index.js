/* globals $, cytoscape, require  */
//function main() {
// #region Cytoscape
let dagre = require("cytoscape-dagre");
cytoscape.use(dagre); // register extension
var cy = cytoscape({
    container: document.getElementById("cy"), // container to render in

    elements: [
        // list of graph elements to start with
        {}
    ],

    style: [
        // the stylesheet for the graph
        {
            selector: "node",
            style: {
                "background-color": "#888",
                label: "data(label)",
                "text-wrap": "wrap",
                "text-max-width": "80px",
                width: "100px",
                height: "100px",
                "min-zoomed-font-size": "8px",
                "background-width": "70%",
                "background-height": "70%"
            }
        },
        {
            selector: "edge",
            style: {
                width: 3,
                "line-color": "#ccc",
                "target-arrow-color": "#ccc",
                "target-arrow-shape": "triangle"
            }
        },
        {
            selector: ".root",
            style: {
                "background-color": "#4dd926"
            }
        },
        {
            selector: ".leaf",
            style: {
                "background-color": "#d93e26"
            }
        },
        {
            selector: ".composite",
            style: {
                //"background-color": "#d6d926",
            }
        },
        {
            selector: ".tree",
            style: {
                "background-color": "#4dd926"
            }
        },
        {
            selector: ".decorator",
            style: {
                //"background-color": "#d98026",
            }
        },
        {
            selector: ".action",
            style: {
                "background-image": "./img/action.svg"
            }
        },
        {
            selector: ".condition",
            style: {
                "background-image": "./img/condition.svg"
            }
        },
        {
            selector: ".sequence",
            style: {
                "background-image": "./img/sequence.svg"
            }
        },
        {
            selector: ".selector",
            style: {
                "background-image": "./img/selector.svg"
            }
        },
        {
            selector: ".negator",
            style: {
                "background-image": "./img/negator.svg"
            }
        },
        {
            selector: ".repeater",
            style: {
                "background-image": "./img/repeater.svg"
            }
        },
        {
            selector: ".repeatUntilFail",
            style: {
                "background-image": "./img/repeatUntilFail.svg"
            }
        },
        {
            selector: ".succeeder",
            style: {
                "background-image": "./img/succeeder.svg"
            }
        },
        {
            selector: ".timer",
            style: {
                "background-image": "./img/timer.svg"
            }
        }
    ],

    // initial viewport state:
    zoom: 1,
    pan: { x: 0, y: 0 },

    // interaction options:
    minZoom: 0.2,
    maxZoom: 5,
    zoomingEnabled: true,
    userZoomingEnabled: true,
    panningEnabled: true,
    userPanningEnabled: true,
    boxSelectionEnabled: false,
    selectionType: "single",
    touchTapThreshold: 8,
    desktopTapThreshold: 4,
    autolock: false,
    autoungrabify: true,
    autounselectify: false,

    // rendering options:
    headless: false,
    styleEnabled: true,
    hideEdgesOnViewport: false,
    hideLabelsOnViewport: false,
    textureOnViewport: false,
    motionBlur: false,
    motionBlurOpacity: 0.2,
    wheelSensitivity: 1,
    pixelRatio: "auto"
});

cy.autoungrabify();

function addNode(nodeId, parentId, nodeType, nodeName) {
    var classes = "";

    switch (nodeType) {
        case "_":
            classes = "root";
            break;
        case "#":
            classes = "tree";
            break;
        case "!":
            classes = "leaf action";
            break;
        case "?":
            classes = "leaf condition";
            break;
        case "&":
            classes = "composite sequence";
            break;
        case "|":
            classes = "composite selector";
            break;
        case "¬":
            classes = "decorator negator";
            break;
        case "n":
            classes = "decorator repeater";
            break;
        case "*":
            classes = "decorator repeatUntilFail";
            break;
        case "^":
            classes = "decorator succeeder";
            break;
        case '"':
            classes = "decorator timer";
            break;
    }

    var node = cy.add({
        group: "nodes",
        data: { id: nodeId, label: nodeName },
        classes: classes
    });

    if (parentId) {
        var edge = cy.add({
            group: "edges",
            data: {
                id: parentId + "to" + nodeId,
                source: parentId,
                target: nodeId
            }
        });
    }

    utility.log(
        `Node ${node.data().id} added as a child of ${
            edge ? edge.data().source || "NULL" : "NULL"
        }`
    );

    // var layout = cy.layout({ name: "breadthfirst", directed: true, roots: "#ROOT" });
    var layout = cy.layout({ name: "dagre", directed: true });
    layout.run();
}

// #endregion

// #region globals
const { ipcRenderer } = require("electron");
const { dialog, app } = require("electron").remote;
const fs = require("fs");

var pathToFileBeingEdited;

/** Start editing a new file */
function newFile() {
    $("#text-editor")[0].value = "";
    pathToFileBeingEdited = undefined;
    parse();
}

/** Open a file from a specified location */
function open() {
    dialog.showOpenDialog(
        {
            filters: [
                { name: "BTML files", extensions: ["btml"] },
                { name: "All Files", extensions: ["*"] }
            ]
        },
        filenames => {
            if (!filenames) {
                utility.log("No file selected");
                return;
            }
            var filename = filenames[0];

            fs.readFile(filename, "utf-8", (err, data) => {
                if (err) {
                    utility.error(
                        "An error ocurred reading the file: " + err.message
                    );
                    return;
                }
                pathToFileBeingEdited = filename;
                // Do stuff with open file
                $("#text-editor")[0].value = data;
                $("#text-editor").change();
            });
        }
    );
}

/** Save file to the path it was loaded from */
function save() {
    if (!pathToFileBeingEdited) {
        saveAs();
    } else {
        var content = $("#text-editor")[0].value;

        fs.writeFile(pathToFileBeingEdited, content, err => {
            if (err) {
                utility.error(
                    "An error ocurred creating the file " + err.message
                );
            }

            utility.log("The file has been succesfully saved");
        });
    }
}

/** Save file to a new path */
function saveAs() {
    var content = $("#text-editor")[0].value;

    dialog.showSaveDialog(
        {
            filters: [
                { name: "BTML files", extensions: ["btml"] },
                { name: "All Files", extensions: ["*"] }
            ]
        },
        filename => {
            if (filename === undefined) {
                utility.log("No file selected");
                return;
            }

            fs.writeFile(filename, content, err => {
                if (err) {
                    utility.error(
                        "An error ocurred creating the file " + err.message
                    );
                }
                pathToFileBeingEdited = filename;
                utility.log("The file has been succesfully saved");
            });
        }
    );
}

/** Modify graph zoom */
function zoom(type) {
    // TODO
    switch (type) {
        case "increase":
            // TODO
            cy.zoom(cy.zoom() + 1);
            break;

        case "decrease":
            // TODO
            cy.zoom(cy.zoom() - 1);
            break;
    }
    cy.center();
}

// #endregion

// #region Event handlers

$(window).resize(() => {
    $("#text-editor")[0].style.width = "calc(100% - 8px)";
    $("#text-editor")[0].style.height = "calc(100% - 5px)";
});

$("#text-editor").change(() => {
    parse();
});

/** Run BTML parser */
$("#output").click(() => {
    var language = "C#";

    var filepath;

    // If we are not using a file we opened, create a temp file to read from there
    if (!pathToFileBeingEdited) {
        filepath = app.getPath("temp") + "\\" + new Date().getTime() + ".btml";
        fs.writeFile(filepath, $("#text-editor")[0].value, "utf-8", () => {
            outputToFile(language, filepath);
        });
    } else {
        filepath = pathToFileBeingEdited;
        outputToFile(language, filepath);
    }

    function outputToFile(l, p) {
        // Run a local copy of the parser
        var executablePath = ".\\BTMLPARSERCPP.exe";
        var parameters = [l, p];
        var child = require("child_process").execFile(
            executablePath,
            parameters,
            (err, stdout, stderr) => {
                if (err) {
                    utility.error(err);
                    return;
                }

                // When the output comes back, if no errors, save that as the dialog
                utility.log(stdout);
                dialog.showSaveDialog(
                    {
                        filters: [
                            { name: "Text files", extensions: ["txt"] },
                            { name: "All Files", extensions: ["*"] }
                        ]
                    },
                    filename => {
                        if (filename === undefined) {
                            utility.log("No file selected");
                            return;
                        }

                        fs.writeFile(filename, stdout, err => {
                            if (err) {
                                utility.error(
                                    "An error ocurred creating the file " +
                                        err.message
                                );
                            }
                            pathToFileBeingEdited = filename;
                            utility.log("The file has been succesfully saved");
                        });
                    }
                );
            }
        );
    }
});

ipcRenderer.on("new", () => {
    newFile();
});

ipcRenderer.on("open", () => {
    open();
});

ipcRenderer.on("save", () => {
    save();
});

ipcRenderer.on("saveAs", () => {
    saveAs();
});

ipcRenderer.on("zoom", (e, type) => {
    zoom(type);
});

// #endregion

// #region Parser functions
function countTabs(s) {
    var num = 0;
    while (s[0] === "\t" || s.substr(0, 4) === "    ") {
        s = s.substr(1);
        num++;
    }
    return num;
}

function addNodesToParent(content, parentId, parentType) {
    var tabNum = 0;
    var nodeType;
    var nodeName;
    var parents = [{ id: parentId, childNo: 0, type: parentType }];
    var parent;

    var lines = content.split("\n");

    var nodeId;

    lines.forEach(line => {
        if (line) {
            // Make sure we replace spaces and tabs
            // Might add as an option
            line = line.replace(/ {4}/g, "\t");

            tabNum = countTabs(line);
            while (tabNum < parents.length - 1) {
                // We finished in this level, so go back to the previous parent
                parents.pop();
            }

            parent = parents[parents.length - 1];
            nodeId =
                parent.id === "ROOT" ? "0" : parent.id + "." + parent.childNo;

            // Adding a child to this parent
            parents[parents.length - 1].childNo++;

            // Get string until first space, that should be the node symbol
            nodeType = line.substring(0, line.indexOf(" ")).trim();

            if ($.isNumeric(nodeType)) nodeType = "n";

            nodeName = line.substring(line.indexOf(" ") + 1);

            // Add a node to the node graph
            addNode(nodeId, parent.id, nodeType, nodeName);

            // Make sure we push this parent into the list for next child
            if (
                ["&", "|", "?", "¬", "n", "*", "^", '"', "#"].includes(nodeType)
            ) {
                parents.push({ id: nodeId, childNo: 0, type: nodeType });
            }

            if (nodeType === "#" && pathToFileBeingEdited) {
                var substreeFilename =
                    pathToFileBeingEdited.substr(
                        0,
                        pathToFileBeingEdited.lastIndexOf("\\") + 1
                    ) + nodeName;
                var content = fs.readFileSync(substreeFilename, "utf-8");
                addNodesToParent(content, nodeId, nodeType);
            }
        }
    });
}

/**
 * Format the text and create a map according to it
 */
function parse() {
    cy.elements().remove();
    addNode("ROOT", "", "_", "Root");
    addNodesToParent($("#text-editor")[0].value, "ROOT", "_");
}

// #endregion

// #region Utility functions
var utility = {
    error: message => {
        console.error(message); // eslint-disable-line
    },

    log: message => {
        console.log(message); // eslint-disable-line
    }
};

// #endregion

newFile();
//}

//main();
