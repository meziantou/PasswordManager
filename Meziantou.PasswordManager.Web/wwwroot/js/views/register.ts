import { FormComponent, InitializeOptions } from "../ui/form-component";
import { required, dataType, DataType } from "../data-annotations";
import { UserService } from "../models/services";
import { Router } from '../router';
import { userMustBeAnonymous } from './utilities';
import { InitializeResult } from '../ui/view-component';

export class UserRegisterForm extends FormComponent<UserRegisterModel> {
    constructor(private userService: UserService, private router: Router) {
        super();
    }

    public async initialize(options?: InitializeOptions) {
        const result = await userMustBeAnonymous(this.userService, this.router);
        if (result !== InitializeResult.Ok) {
            return result;
        }

        return super.initialize(options);
    }

    public loadDataCore() {
        return new UserRegisterModel();
    }

    protected async onSubmit(model: UserRegisterModel) {
        try {
            await this.userService.register(model.email, model.password);
            await this.userService.login(model.email, model.password);
            const user = await this.userService.me();

            this.router.setUrl("/user/generate-key");
            return true;
        } catch (ex) {
            console.error(ex);
            return false;
        }
    }
}

class UserRegisterModel {
    @required
    @dataType(DataType.emailAddress)
    email: string = "";

    @required
    @dataType(DataType.password)
    password: string = "";
}