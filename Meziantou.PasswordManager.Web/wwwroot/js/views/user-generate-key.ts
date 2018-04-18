import { FormComponent, InitializeOptions } from "../ui/form-component";
import { required, dataType, DataType, lookupValues, editable } from "../data-annotations";
import { Router } from '../router';
import * as crypto from '../crypto';
import { IValidationRule, validationRule, CompareOtherPropertyValidationRule } from '../validation/validation';
import { isNumber } from '../utilities';
import { UserService } from '../models/services';
import { InitializeResult } from '../ui/view-component';

export class UserGenerateKeyForm extends FormComponent<UserGenerateKeyModel> {
    constructor(private router: Router, private userService: UserService) {
        super();
    }

    public async initialize(options?: InitializeOptions) {
        if (!this.userService.isAuthenticated()) {
            this.router.setUrl("/login");
            return InitializeResult.StopProcessing;
        }

        return await super.initialize(options);
    }

    public loadDataCore() {
        return new UserGenerateKeyModel();
    }

    protected async onSubmit(model: UserGenerateKeyModel) {
        const key = await crypto.generateRsaKey();
        const publicKey = await crypto.exportPublicKey(key);
        const privateKey = await crypto.exportPrivateKey(key, model.password);

        await this.userService.saveKey(publicKey, privateKey);

        this.router.setUrl("/documents");
        return true;
    }
}

class KeySizeValidationRule implements IValidationRule {
    evaluate(_: any, value: any, key: string): string | null {
        if (!isNumber(value)) {
            return "Must be a number";
        }

        if (value % 8 !== 0) {
            return "Must be a multiple of 8";
        }

        if (value < 256 || value > 16384) {
            return "Must be a multiple of 8 between 256 and 16384";
        }

        return null;
    }
}

class UserGenerateKeyModel {
    @editable
    @validationRule(new KeySizeValidationRule())
    keySize: number = 4096;

    @required
    @dataType(DataType.password)
    password: string = "";

    @dataType(DataType.password)
    @validationRule(new CompareOtherPropertyValidationRule<UserGenerateKeyModel>("password"))
    confirmPassword: string = "";
}