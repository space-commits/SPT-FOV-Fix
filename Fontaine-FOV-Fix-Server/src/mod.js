"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const ScopeTemplates = require("../db/ScopeTemplates.json");
class Mod {
    postDBLoad(container) {
        const logger = container.resolve("WinstonLogger");
        const tables = container.resolve("DatabaseServer").getTables();
        const itemDB = tables.templates.items;
        for (let i in itemDB) {
            let serverItem = itemDB[i];
            if (serverItem._id === "5c07dd120db834001c39092d" || serverItem._id === "5c0a2cec0db834001b7ce47d" || serverItem._parent === "55818add4bdc2d5b648b456f" || serverItem._parent === "55818ae44bdc2dde698b456c" || serverItem._parent === "55818ae44bdc2dde698b456c" || serverItem._parent === "55818aeb4bdc2ddc698b456a") {
                for (let j in ScopeTemplates) {
                    let fileItem = ScopeTemplates[j];
                    if (fileItem.ItemID === serverItem._id) {
                        if (serverItem._id === "5d0a3a58d7ad1a669c15ca14" || serverItem._id === "5d0a3e8cd7ad1a6f6a3d35bd") {
                            serverItem._props.Zooms = [[fileItem.MinZoom, fileItem.MinZoom, fileItem.MaxZoom, fileItem.MaxZoom]];
                        }
                        else if (serverItem._id === "57aca93d2459771f2c7e26db" || serverItem._id === "57ac965c24597706be5f975c" || serverItem._id === "5c0517910db83400232ffee5") {
                            serverItem._props.Zooms = [[fileItem.MaxZoom, fileItem.MinZoom]];
                        }
                        else if (fileItem.IsFixed == true && fileItem.CanToggle == false) {
                            serverItem._props.Zooms = [[fileItem.MinZoom]];
                        }
                        else {
                            serverItem._props.Zooms = [[fileItem.MinZoom, fileItem.MaxZoom]];
                        }
                        for (let k in serverItem._props.AimSensitivity[0]) {
                            serverItem._props.AimSensitivity[0][k] = 0.7;
                        }
                        serverItem._props.HasShoulderContact = fileItem.IsFixed;
                        serverItem._props.ToolModdable = fileItem.CanToggle;
                    }
                }
            }
        }
    }
}
module.exports = { mod: new Mod() };
