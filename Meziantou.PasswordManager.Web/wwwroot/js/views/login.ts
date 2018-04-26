import { FormComponent, InitializeOptions } from "../ui/form-component";
import { required, dataType, DataType } from "../data-annotations";
import { UserService } from "../models/services";
import { Router } from '../router';
import { userMustBeAnonymous } from './utilities';
import { InitializeResult } from '../ui/view-component';

export class LoginView extends FormComponent<LoginModel> {
    constructor(private userService: UserService, private router: Router) {
        super();
    }

    public async initialize(options?: InitializeOptions) {
        if (await userMustBeAnonymous(this.userService, this.router) === InitializeResult.StopProcessing) {
            return InitializeResult.StopProcessing;
        }

        return super.initialize(options);
    }

    protected loadDataCore() {
        return new LoginModel();
    }

    protected async onSubmit(model: LoginModel) {
        try {
            if (await this.userService.login(model.email, model.password)) {
                this.router.setUrl("/documents");
                return true;
            }

            return false;
        } catch (ex) {
            console.error(ex);
            return false;
        }
    }
}

class LoginModel {
    @required
    @dataType(DataType.emailAddress)
    email: string = "";

    @required
    @dataType(DataType.password)
    password: string = "";
}