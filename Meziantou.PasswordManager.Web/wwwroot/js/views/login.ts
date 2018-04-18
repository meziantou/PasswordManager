import { FormComponent } from "../ui/form-component";
import { required, dataType, DataType } from "../data-annotations";
import { UserService } from "../models/services";
import { Router } from '../router';

export class LoginView extends FormComponent<LoginModel> {
    constructor(private userService: UserService, private router: Router) {
        super();
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