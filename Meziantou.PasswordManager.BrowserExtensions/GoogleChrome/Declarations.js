"use strict";
var Meziantou;
(function (Meziantou) {
    var PasswordManager;
    (function (PasswordManager) {
        var FieldValueType;
        (function (FieldValueType) {
            FieldValueType[FieldValueType["String"] = 0] = "String";
            FieldValueType[FieldValueType["MultiLineString"] = 1] = "MultiLineString";
            FieldValueType[FieldValueType["Username"] = 2] = "Username";
            FieldValueType[FieldValueType["Password"] = 3] = "Password";
            FieldValueType[FieldValueType["Url"] = 4] = "Url";
            FieldValueType[FieldValueType["EmailAddress"] = 5] = "EmailAddress";
        })(FieldValueType = PasswordManager.FieldValueType || (PasswordManager.FieldValueType = {}));
    })(PasswordManager = Meziantou.PasswordManager || (Meziantou.PasswordManager = {}));
})(Meziantou || (Meziantou = {}));
